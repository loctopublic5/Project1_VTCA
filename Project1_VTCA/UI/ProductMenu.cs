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

        public async Task ShowAllProductsAsync()
        {
            int currentPage = 1;
            const int PageSize = 10;
            string currentSortBy = "default";
            decimal? currentMinPrice = null;
            decimal? currentMaxPrice = null;

            while (true)
            {
                var menuContent = new Markup(
                    "[bold yellow underline]SẮP XẾP[/]\n" +
                    "1. Mặc định (Mới nhất)\n" +
                    "2. Giá: Cao đến thấp\n" +
                    "3. Giá: Thấp đến cao\n\n" +
                    "[bold yellow underline]LỌC & TÌM KIẾM[/]\n" +
                    "4. Tìm kiếm theo tên\n" +
                    "5. Lọc theo danh mục\n" +
                    "6. Lọc theo khoảng giá\n\n" +
                    "[dim]n - Trang sau; p - Trang trước; p.{số trang} - Đến trang nhất định; id.{mã id} - Xem chi tiết sp; exit - Thoát[/]"
                );

                var (products, totalPages) = await _productService.GetActiveProductsPaginatedAsync(currentPage, PageSize, currentSortBy, currentMinPrice, currentMaxPrice);
                totalPages = totalPages > 0 ? totalPages : 1;

                var productTable = await CreateProductTableAsync(products);

                var notificationText = $"Trang [bold yellow]{currentPage}[/] / [bold yellow]{totalPages}[/]\n" +
                                       "Lệnh: [blue]'n'[/](Sau), [blue]'p'[/](Trước), [blue]'p.{số}'[/](Đến trang), [red]'exit'[/] hoặc chọn Menu";

                _layout.Render(menuContent, productTable, new Markup($"[bold]{notificationText}[/]"));

                Console.Write("\n> Nhập lệnh: ");
                string choice = Console.ReadLine()?.ToLower().Trim() ?? "";

                if (choice.StartsWith("id."))
                {
                    if (int.TryParse(choice.AsSpan(3), out int productId))
                    {
                        await HandleViewProductDetailsAsync(productId);
                    }
                }
                else
                {
                    if (!HandleChoice(choice, ref currentPage, ref currentSortBy, ref currentMinPrice, ref currentMaxPrice, totalPages)) return;
                }
            }
        }

        // Trả về bool để quyết định có tiếp tục vòng lặp hay không
        private bool HandleChoice(string choice, ref int currentPage, ref string sortBy, ref decimal? minPrice, ref decimal? maxPrice, int totalPages)
        {
            switch (choice)
            {
                case "exit": return false; // Báo hiệu cần thoát
                case "n": if (currentPage < totalPages) currentPage++; break;
                case "p": if (currentPage > 1) currentPage--; break;
                case "1": minPrice = null; maxPrice = null; sortBy = "default"; currentPage = 1; break;
                case "2": minPrice = null; maxPrice = null; sortBy = "price_desc"; currentPage = 1; break;
                case "3": minPrice = null; maxPrice = null; sortBy = "price_asc"; currentPage = 1; break;
                case "4": HandleSearchAsync().GetAwaiter().GetResult(); break;
                case "5": HandleCategoryFilterAsync().GetAwaiter().GetResult(); break;
                case "6":
                    var (min, max) = HandlePriceFilterAsync().GetAwaiter().GetResult();
                    minPrice = min; maxPrice = max;
                    sortBy = "price_asc"; currentPage = 1;
                    break;

                default:
                    if (choice.StartsWith("p.") && int.TryParse(choice.AsSpan(2), out int targetPage) && targetPage >= 1 && targetPage <= totalPages)
                    {
                        currentPage = targetPage;
                    }
                    break;
            }
            return true; // Báo hiệu tiếp tục vòng lặp
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



        private async Task HandleCategoryFilterAsync()
        {
            var allCategories = await _productService.GetAllProductCategoriesAsync();
            if (!allCategories.Any())
            {
                AnsiConsole.MarkupLine("[red]Không có danh mục nào để lọc.[/]");
                Console.ReadKey();
                return;
            }

            var selectedCategories = AnsiConsole.Prompt(
                new MultiSelectionPrompt<Category>()
                    .Title("Chọn các [green]danh mục[/] bạn muốn lọc (dùng [blue]phím cách[/] để chọn, [green]enter[/] để xác nhận):")
                    .PageSize(10)
                    .MoreChoicesText("[grey](Dùng phím Lên/Xuống để di chuyển)[/]")
                    .UseConverter(c => c.Name)
                    .AddChoices(allCategories));

            if (!selectedCategories.Any())
            {
                AnsiConsole.MarkupLine("[yellow]Bạn chưa chọn danh mục nào.[/]");
                Console.ReadKey();
                return;
            }

            var selectedCategoryIds = selectedCategories.Select(c => c.CategoryID).ToList();
            var selectedCategoryNames = string.Join(", ", selectedCategories.Select(c => c.Name));

            // Bắt đầu vòng lặp hiển thị kết quả lọc với phân trang
            int currentPage = 1;
            const int PageSize = 10;

            while (true)
            {
                var (products, totalPages) = await _productService.GetProductsByCategoriesPaginatedAsync(selectedCategoryIds, currentPage, PageSize);
                totalPages = totalPages > 0 ? totalPages : 1;

                var resultTable = await CreateProductTableAsync(products);
                var menuContent = new Markup($"[bold yellow underline]ĐANG LỌC THEO[/]\n[cyan]{selectedCategoryNames}[/]");
                var notificationText = $"Trang [bold yellow]{currentPage}[/] / [bold yellow]{totalPages}[/]\n" +
                                       "[bold]Lệnh: [blue]'n'[/](Sau), [blue]'p'[/](Trước), [blue]'p.{số}'[/](Đến trang), [red]'exit'[/][/]";

                _layout.Render(menuContent, resultTable, new Markup(notificationText));

                Console.Write("\n> Nhập lệnh: ");
                string choice = Console.ReadLine()?.ToLower().Trim() ?? "";

                if (choice == "exit") return; // Thoát khỏi màn hình lọc, quay về menu duyệt sản phẩm chính
                if (choice == "n" && currentPage < totalPages) currentPage++;
                if (choice == "p" && currentPage > 1) currentPage--;
                if (choice.StartsWith("p.") && choice.Length > 2)
                {
                    if (int.TryParse(choice.Substring(2), out int targetPage) && targetPage >= 1 && targetPage <= totalPages)
                    {
                        currentPage = targetPage;
                    }
                }
            }
        }
        private async Task HandleSearchAsync()
        {
            // Vòng lặp tìm kiếm bên ngoài
            while (true)
            {
                var menuContent = new Markup("[dim]Nhập từ khóa tìm kiếm.\nTừ khóa đầu tiên không được để trống.[/]");
                var searchView = new Text("");
                var searchNotification = new Markup("[dim]Nhập '[red]exit[/]' để quay về menu duyệt sản phẩm.[/]");
                _layout.Render(menuContent, searchView, searchNotification);

                Console.Write("\n> Nhập từ khóa tìm kiếm: ");
                string searchTerm = Console.ReadLine()?.Trim() ?? "";

                if (searchTerm.Equals("exit", StringComparison.OrdinalIgnoreCase))
                {
                    return; // Thoát hoàn toàn khỏi chức năng tìm kiếm
                }

                if (string.IsNullOrWhiteSpace(searchTerm) || searchTerm.Split(' ')[0] == "")
                {
                    AnsiConsole.MarkupLine("[red]Từ khóa tìm kiếm không hợp lệ. Nhấn phím bất kỳ để nhập lại.[/]");
                    Console.ReadKey();
                    continue; // Quay lại đầu vòng lặp tìm kiếm
                }

                // Bắt đầu phân trang cho kết quả tìm kiếm
                int currentPage = 1;
                while (true)
                {
                    var (products, totalPages) = await _productService.SearchProductsAsync(searchTerm, currentPage, 10);
                    totalPages = totalPages > 0 ? totalPages : 1;

                    if (!products.Any() && currentPage == 1)
                    {
                        _layout.Render(menuContent, new Text("Không tìm thấy sản phẩm nào phù hợp.", new Style(Color.Red)).Centered(), searchNotification);
                        AnsiConsole.Markup("[dim]Nhấn phím bất kỳ để tìm kiếm lại...[/]");
                        Console.ReadKey();
                        break; // Thoát khỏi vòng lặp phân trang, quay lại vòng lặp tìm kiếm
                    }

                    var resultTable = await CreateProductTableAsync(products);
                    var searchMenuContent = new Markup($"[dim]Kết quả tìm kiếm cho:\n[yellow]'{Markup.Escape(searchTerm)}'[/][/]");
                    var notificationText = $"Trang [bold yellow]{currentPage}[/] / [bold yellow]{totalPages}[/]\n" +
                                           "Lệnh: [blue]'n'[/](Sau), [blue]'p'[/](Trước), [blue]'p.{số}'[/], [red]'exit'[/](Tìm kiếm lại)";

                    _layout.Render(searchMenuContent, resultTable, new Markup(notificationText));

                    Console.Write("\n> Nhập lệnh: ");
                    string navChoice = Console.ReadLine()?.ToLower().Trim() ?? "";

                    if (navChoice == "exit") break; // Thoát khỏi vòng lặp phân trang, quay lại vòng lặp tìm kiếm
                    if (navChoice == "n" && currentPage < totalPages) currentPage++;
                    if (navChoice == "p" && currentPage > 1) currentPage--;
                    if (navChoice.StartsWith("p.") && int.TryParse(navChoice.AsSpan(2), out int targetPage) && targetPage >= 1 && targetPage <= totalPages)
                    {
                        currentPage = targetPage;
                    }
                }
            }






        }
    }
}