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
                    "7. Lọc theo khuyến mãi\n\n" +
                    "[dim]Nhập lựa chọn và nhấn Enter.[/]"
                );

                var (products, totalPages) = await _productService.GetActiveProductsPaginatedAsync(currentPage, PageSize, currentSortBy, currentMinPrice, currentMaxPrice);
                totalPages = totalPages > 0 ? totalPages : 1;

                var productTable = await CreateProductTableAsync(products);

                var notificationText = $"Trang [bold yellow]{currentPage}[/] / [bold yellow]{totalPages}[/]\n" +
                                       "Lệnh: [blue]'n'[/](Sau), [blue]'p'[/](Trước), [blue]'p.{số}'[/](Đến trang), [red]'exit'[/] hoặc chọn Menu";

                _layout.Render(menuContent, productTable, new Markup($"[bold]{notificationText}[/]"));

                Console.Write("\n> Nhập lệnh: ");
                string choice = Console.ReadLine()?.ToLower().Trim() ?? "";

                if (!HandleChoice(choice, ref currentPage, ref currentSortBy, ref currentMinPrice, ref currentMaxPrice, totalPages))
                {
                    // Nếu HandleChoice trả về false (tức là người dùng chọn exit hoặc một hành động thoát khác)
                    return;
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
                case "7":
                    await HandleDiscountedFilterAsync(); // <-- Gọi hàm xử lý mới
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
            var menuContent = new Markup("[dim]Nhập từ khóa và nhấn Enter.\nNhập '[red]exit[/]' để quay lại.[/]");
            _layout.Render(menuContent, new Text(""), new Markup("[dim]Đang chờ từ khóa...[/]"));

            Console.Write("\n> Nhập từ khóa tìm kiếm: ");
            string searchTerm = Console.ReadLine()?.Trim() ?? "";
            if (string.IsNullOrEmpty(searchTerm) || searchTerm.Equals("exit", StringComparison.OrdinalIgnoreCase)) return;

            int currentPage = 1;
            while (true)
            {
                var (products, totalPages) = await _productService.SearchProductsAsync(searchTerm, currentPage, 10);
                totalPages = totalPages > 0 ? totalPages : 1;
                var resultTable = await CreateProductTableAsync(products);
                var searchMenuContent = new Markup($"[dim]Kết quả tìm kiếm cho:\n[yellow]'{Markup.Escape(searchTerm)}'[/][/]");
                var notificationText = $"Trang [bold yellow]{currentPage}[/] / [bold yellow]{totalPages}[/]\n" +
                                       "[bold]Lệnh: [blue]'n'[/](Sau), [blue]'p'[/](Trước), [blue]'p.{số}'[/](Đến trang), [red]'exit'[/][/]";
                _layout.Render(searchMenuContent, resultTable, new Markup(notificationText));
                Console.Write("\n> Nhập lệnh: ");
                string navChoice = Console.ReadLine()?.ToLower().Trim() ?? "";

                if (navChoice == "exit") return;
                if (navChoice == "n" && currentPage < totalPages) currentPage++;
                if (navChoice == "p" && currentPage > 1) currentPage--;
                if (navChoice.StartsWith("p.") && int.TryParse(navChoice.AsSpan(2), out int targetPage) && targetPage >= 1 && targetPage <= totalPages)
                {
                    currentPage = targetPage;
                }
            }
        }

        // --- PHƯƠNG THỨC MỚI ĐỂ LỌC SẢN PHẨM GIẢM GIÁ ---
        private async Task HandleDiscountedFilterAsync()
        {
            var menuContent = new Markup("[bold yellow underline]ĐANG LỌC[/]\n[cyan]Sản phẩm giảm giá[/]");
            int currentPage = 1;

            while (true)
            {
                var (products, totalPages) = await _productService.GetDiscountedProductsPaginatedAsync(currentPage, 10);
                totalPages = totalPages > 0 ? totalPages : 1;
                var resultTable = await CreateProductTableAsync(products);
                var notificationText = $"Trang [bold yellow]{currentPage}[/] / [bold yellow]{totalPages}[/]\n" +
                                       "[bold]Lệnh: [blue]'n'[/](Sau), [blue]'p'[/](Trước), [blue]'p.{số}'[/](Đến trang), [red]'exit'[/][/]";
                _layout.Render(menuContent, resultTable, new Markup(notificationText));
                Console.Write("\n> Nhập lệnh: ");
                string navChoice = Console.ReadLine()?.ToLower().Trim() ?? "";

                if (navChoice == "exit") return;
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