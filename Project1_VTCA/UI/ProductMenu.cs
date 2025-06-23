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
                    "9. Bỏ tất cả bộ lọc\n\n" +
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
                case "9":
                    minPrice = null; maxPrice = null;
                    sortBy = "default"; currentPage = 1;
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




        // --- PHƯƠNG THỨC MỚI ĐỂ LỌC GIÁ ---
        private async Task<(decimal?, decimal?)> HandlePriceFilterAsync()
        {
            AnsiConsole.Clear();
            Banner.Show();

            var choice = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("[bold yellow]CHỌN KHOẢNG GIÁ BẠN MUỐN LỌC[/]")
                    .AddChoices(new[]
                    {
                        "Dưới 1.000.000 VNĐ",
                        "Từ 1.000.000 - 2.500.000 VNĐ",
                        "Trên 2.500.000 VNĐ",
                        "Nhập khoảng giá tùy chỉnh",
                        "[red]Hủy bỏ[/]"
                    }));

            switch (choice)
            {
                case "Dưới 1.000.000 VNĐ":
                    return (null, 999999);
                case "Từ 1.000.000 - 2.500.000 VNĐ":
                    return (1000000, 2500000);
                case "Trên 2.500.000 VNĐ":
                    return (2500001, null);
                case "Nhập khoảng giá tùy chỉnh":
                    var min = AnsiConsole.Ask<decimal>("Nhập giá tối thiểu (số):");
                    var max = AnsiConsole.Ask<decimal>("Nhập giá tối đa (số):");
                    if (min > max)
                    {
                        AnsiConsole.MarkupLine("[red]Giá tối thiểu không thể lớn hơn giá tối đa.[/]");
                        Console.ReadKey();
                        return (null, null);
                    }
                    return (min, max);
                case "[red]Hủy bỏ[/]":
                default:
                    return (null, null);
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
            while (true)
            {
                // Vẽ một layout riêng cho việc tìm kiếm
                var searchMenu = new Markup("[dim]Nhập từ khóa tìm kiếm.\nVí dụ: [yellow]nike air[/], [yellow]vans[/]...\nTừ khóa đầu tiên không được để trống.[/]");
                var searchView = new Text("Kết quả tìm kiếm sẽ hiện ở đây.");
                var searchNotification = new Markup("[dim]Nhập '[red]exit[/]' để quay lại menu duyệt sản phẩm.[/]");
                _layout.Render(searchMenu, searchView, searchNotification);

                Console.Write("\n> Nhập từ khóa tìm kiếm: ");
                string searchTerm = Console.ReadLine()?.Trim() ?? "";

                if (searchTerm.Equals("exit", StringComparison.OrdinalIgnoreCase))
                {
                    return; // Thoát về menu duyệt sản phẩm
                }

                // Kiểm tra điều kiện: ký tự đầu tiên trước dấu cách phải có
                if (string.IsNullOrWhiteSpace(searchTerm) || searchTerm.Split(' ')[0] == "")
                {
                    AnsiConsole.MarkupLine("[red]Từ khóa tìm kiếm không hợp lệ. Vui lòng nhập lại.[/]");
                    Console.ReadKey();
                    continue; // Quay lại vòng lặp tìm kiếm
                }

                // Gọi service để tìm sản phẩm
                var results = await _productService.SearchProductsAsync(searchTerm);
                var resultTable = await CreateProductTableAsync(results); // Tái sử dụng hàm vẽ bảng

                if (!results.Any())
                {
                    _layout.Render(searchMenu, new Text("Không tìm thấy sản phẩm nào phù hợp.", new Style(Color.Red)).Centered(), searchNotification);
                }
                else
                {
                    _layout.Render(searchMenu, resultTable, new Markup($"[green]Tìm thấy {results.Count} sản phẩm phù hợp.[/]"));
                }

                AnsiConsole.Markup("[dim]Nhấn phím bất kỳ để tìm kiếm lại...[/]");
                Console.ReadKey();
            }
        }

       

    }
}