using Microsoft.EntityFrameworkCore;
using Project1_VTCA.Data;
using Project1_VTCA.Services;
using Project1_VTCA.Utils;
using Spectre.Console;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project1_VTCA.UI
{
    public class ProductMenu
    {
        private readonly IProductService _productService;
        private readonly IPromotionService _promotionService;
        private readonly ISessionService _sessionService;
        private readonly ConsoleLayout _layout;

        public ProductMenu(IProductService productService, IPromotionService promotionService, ISessionService sessionService, ConsoleLayout layout)
        {
            _productService = productService;
            _promotionService = promotionService;
            _sessionService = sessionService;
            _layout = layout;
        }

        private class ProductDisplayState
        {
            public int CurrentPage { get; set; } = 1;
            public int PageSize { get; set; } = 10;
            public int TotalPages { get; set; } = 1;
            public string SortBy { get; set; } = "default";
            public decimal? MinPrice { get; set; } = null;
            public decimal? MaxPrice { get; set; } = null;
            public List<int> CategoryIds { get; set; } = null;
            public string SearchTerm { get; set; } = null;

            public void Reset()
            {
                CurrentPage = 1;
                SortBy = "default";
                MinPrice = null;
                MaxPrice = null;
                CategoryIds = null;
                SearchTerm = null;
            }
        }

        public async Task<bool> ShowAllProductsAsync()
        {
            var state = new ProductDisplayState();

            while (true)
            {
                // 1. XÂY DỰNG CÂU TRUY VẤN DỰA TRÊN TRẠNG THÁI HIỆN TẠI
                var query = _productService.GetActiveProductsQuery();

                // Áp dụng bộ lọc tìm kiếm
                if (!string.IsNullOrEmpty(state.SearchTerm))
                {
                    var keywords = state.SearchTerm.ToLower().Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    foreach (var keyword in keywords)
                    {
                        query = query.Where(p => p.Name.ToLower().Contains(keyword));
                    }
                }

                // Áp dụng bộ lọc danh mục
                if (state.CategoryIds != null && state.CategoryIds.Any())
                {
                    query = query.Where(p => p.ProductCategories.Any(pc => state.CategoryIds.Contains(pc.CategoryID)));
                }

                // Áp dụng bộ lọc giá
                if (state.MinPrice.HasValue) query = query.Where(p => p.Price >= state.MinPrice.Value);
                if (state.MaxPrice.HasValue) query = query.Where(p => p.Price <= state.MaxPrice.Value);

                // 2. LẤY DỮ LIỆU ĐÃ PHÂN TRANG VÀ SẮP XẾP
                var (products, totalPages) = await _productService.GetPaginatedProductsAsync(query, state.CurrentPage, state.PageSize, state.SortBy);
                state.TotalPages = totalPages;

                // 3. VẼ GIAO DIỆN
                var menuContent = CreateSideMenu();
                var productTable = await CreateProductTableAsync(products);
                var notificationText = $"Trang [bold yellow]{state.CurrentPage}[/] / [bold yellow]{state.TotalPages}[/]\n" +
                                       "Lệnh: [blue]'n'[/](Sau), [blue]'p'[/](Trước), [yellow]'id.{số}'[/], [red]'exit'[/] hoặc chọn Menu";
                _layout.Render(menuContent, productTable, new Markup($"[bold]{notificationText}[/]"));

                // 4. NHẬN VÀ XỬ LÝ LỆNH
                Console.Write("\n> Nhập lệnh: ");
                string choice = Console.ReadLine()?.ToLower().Trim() ?? "";

                if (await HandleChoice(choice, state) == false)
                {
                    return false; // Người dùng muốn thoát về menu chính của Customer
                }
            }
        }

        private async Task<bool> HandleChoice(string choice, ProductDisplayState state)
        {
            if (choice.StartsWith("id."))
            {
                if (int.TryParse(choice.AsSpan(3), out int productId))
                {
                    // Khi xem chi tiết xong, ta muốn quay lại đúng màn hình lọc/tìm kiếm hiện tại
                    await HandleViewProductDetailsAsync(productId);
                }
                return true; // Báo hiệu tiếp tục vòng lặp
            }

            switch (choice)
            {
                case "exit": return false;
                case "0": state.Reset(); break;
                case "n": if (state.CurrentPage < state.TotalPages) state.CurrentPage++; break;
                case "p": if (state.CurrentPage > 1) state.CurrentPage--; break;
                case "1": state.SortBy = "default"; state.CurrentPage = 1; break;
                case "2": state.SortBy = "price_desc"; state.CurrentPage = 1; break;
                case "3": state.SortBy = "price_asc"; state.CurrentPage = 1; break;
                case "4":
                    state.SearchTerm = await HandleSearchPrompt();
                    state.CurrentPage = 1;
                    break;
                case "5":
                    state.CategoryIds = await HandleCategoryFilterAsync();
                    state.CurrentPage = 1;
                    break;
                case "6":
                    var (min, max) = await HandlePriceFilterAsync();
                    state.MinPrice = min; state.MaxPrice = max;
                    state.SortBy = "price_asc"; state.CurrentPage = 1;
                    break;
                default:
                    if (choice.StartsWith("p.") && int.TryParse(choice.AsSpan(2), out int targetPage) && targetPage >= 1 && targetPage <= state.TotalPages)
                    {
                        state.CurrentPage = targetPage;
                    }
                    break;
            }
            return true; // Báo hiệu tiếp tục vòng lặp
        }

        // Hàm tạo menu bên trái
        private Markup CreateSideMenu()
        {
            return new Markup(
                "[bold yellow underline]SẮP XẾP[/]\n" +
                "1. Mặc định\n" +
                "2. Giá: Cao-Thấp\n" +
                "3. Giá: Thấp-Cao\n\n" +
                "[bold yellow underline]LỌC & TÌM KIẾM[/]\n" +
                "4. Tìm kiếm\n" +
                "5. Lọc theo danh mục\n" +
                "6. Lọc theo giá\n" +
                "0. Trang chính\n\n" +
                "[dim]Nhập lựa chọn và nhấn Enter.[/]"
            );
        }

        // Các hàm Handle... giờ chỉ chuẩn bị truy vấn và gọi hàm hiển thị trung tâm
        private async Task HandleSearchAsync()
        {
            Console.Write("\n> Nhập từ khóa tìm kiếm (hoặc 'exit'): ");
            string searchTerm = Console.ReadLine()?.Trim() ?? "";
            if (string.IsNullOrEmpty(searchTerm) || searchTerm.Equals("exit", StringComparison.OrdinalIgnoreCase)) return;

            var query = _productService.GetSearchQuery(searchTerm);
            await DisplayProductListAsync(query, $"TÌM KIẾM: '{searchTerm}'");
        }

        private async Task<List<int>> HandleCategoryFilterAsync()
        {
            var allCategories = await _productService.GetAllProductCategoriesAsync();
            if (!allCategories.Any())
            {
                AnsiConsole.MarkupLine("[red]Lỗi: Không có danh mục nào.[/]");
                Console.ReadKey();
                return new List<int>();
            }

            var selectedCategories = AnsiConsole.Prompt(
                new MultiSelectionPrompt<Category>()
                    .Title("Chọn [green]danh mục[/] (dùng [blue]phím cách[/] để chọn, [green]enter[/] để xác nhận):")
                    .UseConverter(c => c.Name)
                    .AddChoices(allCategories));

            return selectedCategories.Select(c => c.CategoryID).ToList();
        }



        private async Task HandleViewProductDetailsAsync(int productId)
        {
            var product = await _productService.GetProductByIdAsync(productId);
            if (product == null)
            {
                AnsiConsole.MarkupLine("\n[red]Lỗi: Không tìm thấy sản phẩm với ID này hoặc sản phẩm đã ngừng kinh doanh.[/]");
                Console.ReadKey();
                return;
            }

            while (true)
            {
                var (discountedPrice, promoCode) = await _promotionService.CalculateDiscountedPriceAsync(product);

                var actionMenu = new Markup(
                    "[bold yellow underline]HÀNH ĐỘNG[/]\n" +
                    "1. Mua ngay\n" +
                    "2. Thêm vào giỏ hàng\n\n" +
                    "[red]3. Quay lại danh sách[/]"
                );

                var brand = product.ProductCategories?.Select(pc => pc.Category).FirstOrDefault(c => c.CategoryType == "Brand")?.Name ?? "N/A";
                var styles = string.Join(", ", product.ProductCategories?.Where(pc => pc.Category.CategoryType == "Product").Select(c => c.Category.Name) ?? Enumerable.Empty<string>());
                var priceDisplay = discountedPrice.HasValue
                    ? $"[strikethrough dim red]{product.Price:N0} VNĐ[/]  [bold green]{discountedPrice.Value:N0} VNĐ[/] ([italic]Tiết kiệm: {product.Price - discountedPrice.Value:N0} VNĐ[/])"
                    : $"[bold green]{product.Price:N0} VNĐ[/]";
                var sizes = string.Join(" | ", product.ProductSizes?.Select(s => $"{s.Size} (SL: {s.QuantityInStock})") ?? Enumerable.Empty<string>());

                var detailsPanel = new Panel(
                    new Rows(
                        new FigletText(brand).Color(Color.Orange1),
                        new Text(product.Name, new Style(decoration: Decoration.Bold | Decoration.Underline)).LeftJustified(),
                        new Text(""),
                        new Markup($"[bold]Phong cách:[/] {styles}"),
                        new Markup($"[bold]Giá:[/] {priceDisplay}"),
                        new Rule("[yellow]Mô tả[/]").LeftJustified(),
                        new Padder(new Markup(Markup.Escape(product.Description ?? "")), new Padding(0, 0, 0, 1)),
                        new Rule("[yellow]Size còn hàng[/]").LeftJustified(),
                        new Text(sizes)
                    ))
                    .Header("CHI TIẾT SẢN PHẨM")
                    .Expand();

                var notification = new Markup("[dim]Chọn một hành động từ menu bên trái.[/]");
                _layout.Render(actionMenu, detailsPanel, notification);

                Console.Write("\n> Nhập lựa chọn hành động: ");
                string choice = Console.ReadLine()?.Trim() ?? "";

                switch (choice)
                {
                    case "1":
                        AnsiConsole.MarkupLine("[yellow]Chức năng 'Mua ngay' sẽ được hiện thực sau.[/]");
                        Console.ReadKey();
                        break;
                    case "2":
                        AnsiConsole.MarkupLine("[yellow]Chức năng 'Thêm vào giỏ hàng' sẽ được hiện thực sau.[/]");
                        Console.ReadKey();
                        break;
                    case "3":
                        return;
                }
            }
        }

        private async Task<(decimal?, decimal?)> HandlePriceFilterAsync()
        {
            var menuContent = new Markup(
                    "[bold yellow underline]LỌC THEO GIÁ[/]\n" +
                    "1. Dưới 1.000.000 VNĐ\n" +
                    "2. Từ 1.000.000 - 2.500.000 VNĐ\n" +
                    "3. Trên 2.500.000 VNĐ\n" +
                    "4. Nhập khoảng giá tùy chỉnh\n\n" +
                    "Nhập '[red]exit[/]' để quay lại."
                );
            var viewContent = new Text("Vui lòng chọn một khoảng giá từ menu bên trái.");
            var notificationContent = new Markup("[dim]Nhập lựa chọn của bạn.[/]");

            _layout.Render(menuContent, viewContent, notificationContent);

            Console.Write("\n> Nhập lựa chọn lọc giá: ");
            string choice = Console.ReadLine()?.ToLower().Trim() ?? "";

            switch (choice)
            {
                case "1": return (null, 999999);
                case "2": return (1000000, 2500000);
                case "3": return (2500001, null);
                case "4":
                    // Vẽ lại giao diện để hỏi giá min/max
                    var customMenu = new Markup("[dim]Đang ở chế độ nhập tùy chỉnh.\nNhập '[red]exit[/]' để hủy.[/]");
                    var customView = new Text("Vui lòng nhập khoảng giá bạn muốn.");
                    _layout.Render(customMenu, customView, notificationContent);

                    var min = AnsiConsole.Ask<decimal>("\n> Nhập giá tối thiểu (số):");
                    var max = AnsiConsole.Ask<decimal>("> Nhập giá tối đa (số):");

                    if (min > max)
                    {
                        AnsiConsole.MarkupLine("[red]Lỗi: Giá tối thiểu không thể lớn hơn giá tối đa.[/]");
                        Console.ReadKey();
                        return (null, null);
                    }
                    return (min, max);
                case "exit":
                default:
                    return (null, null); // Trả về không có bộ lọc nếu hủy hoặc chọn sai
            }
        }



        private async Task<Table> CreateProductTableAsync(List<Product> products)
        {
            var table = new Table().Expand().Border(TableBorder.HeavyHead);
            table.Title = new TableTitle("DANH SÁCH SẢN PHẨM");

            table.AddColumn(new TableColumn("[yellow]ID[/]") { Alignment = Justify.Center });
            table.AddColumn(new TableColumn("[yellow]Tên sản phẩm[/]"));
            table.AddColumn(new TableColumn("[yellow]Thương hiệu[/]"));
            table.AddColumn(new TableColumn("[yellow]Phong cách chính[/]")); // Đổi tên cột
            table.AddColumn(new TableColumn("[yellow]Giá[/]") { Alignment = Justify.Right });

            if (!products.Any())
            {
                table.AddRow(new Text("Không có sản phẩm nào để hiển thị.", new Style(Color.Red))).Centered();
                return table;
            }

            foreach (var product in products)
            {
                var (discountedPrice, promoCode) = await _promotionService.CalculateDiscountedPriceAsync(product);
                string priceDisplay = discountedPrice.HasValue
                    ? $"[strikethrough dim red]{product.Price:N0}[/] [bold green]{discountedPrice.Value:N0} VNĐ[/]\n[italic dim]Áp dụng: {promoCode}[/]"
                    : $"[green]{product.Price:N0} VNĐ[/]\n ";

                var brand = product.ProductCategories?.Select(pc => pc.Category).FirstOrDefault(c => c.CategoryType == "Brand")?.Name ?? "N/A";

                // --- LOGIC MỚI: CHỈ LẤY PHONG CÁCH ĐẦU TIÊN ---
                var mainStyle = product.ProductCategories?
                                     .Select(pc => pc.Category)
                                     .FirstOrDefault(c => c.CategoryType == "Product")?.Name ?? "N/A";

                table.AddRow(
                    new Markup(product.ProductID.ToString()),
                    new Markup(Markup.Escape(product.Name)),
                    new Markup(brand),
                    new Markup(mainStyle), // <-- Hiển thị phong cách chính
                    new Markup(priceDisplay)
                );
            }
            return table;
        }
        private async Task DisplayProductListAsync(IQueryable<Product> query, string title)
        {
            var products = await query.ToListAsync();
            var productTable = await CreateProductTableAsync(products);

            var menuContent = CreateSideMenu();
            var notificationContent = new Markup($"[bold]{title}[/]");

            _layout.Render(menuContent, productTable, notificationContent);
        }
        private async Task<List<int>> GetCategoryFilterAsync()
        {
            var allCategories = await _productService.GetAllProductCategoriesAsync();
            if (!allCategories.Any())
            {
                AnsiConsole.MarkupLine("[red]Lỗi: Không có danh mục nào.[/]");
                Console.ReadKey();
                return new List<int>();
            }

            var selectedCategories = AnsiConsole.Prompt(
                new MultiSelectionPrompt<Category>()
                    .Title("Chọn [green]danh mục[/] (dùng [blue]phím cách[/] để chọn, [green]enter[/] để xác nhận):")
                    .UseConverter(c => c.Name)
                    .AddChoices(allCategories));

            return selectedCategories.Select(c => c.CategoryID).ToList();
        }
        private async Task<string> HandleSearchPrompt()
        {
            Console.Write("\n> Nhập từ khóa tìm kiếm (hoặc 'exit'): ");
            string searchTerm = Console.ReadLine()?.Trim() ?? "";
            return searchTerm.Equals("exit", StringComparison.OrdinalIgnoreCase) ? null : searchTerm;
        }



    }
    
}