using Project1_VTCA.Data;
using Project1_VTCA.Services;
using Spectre.Console;
using System;
using System.Collections.Generic;
using System.Linq;
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

            while (true)
            {
                var menuContent = new Markup(
                    "[bold yellow underline]SẮP XẾP[/]\n" +
                    "1. Mặc định (Mới nhất)\n" +
                    "2. Giá: Cao đến thấp\n" +
                    "3. Giá: Thấp đến cao\n\n" +
                    "[bold yellow underline]LỌC & TÌM KIẾM[/]\n" +
                    "4. Tìm kiếm theo tên\n" +
                    "5. Lọc theo danh mục\n\n" +
                    "[dim]Nhập lựa chọn và nhấn Enter.[/]"
                );

                var (products, totalPages) = await _productService.GetActiveProductsPaginatedAsync(currentPage, PageSize, currentSortBy);
                totalPages = totalPages > 0 ? totalPages : 1;

                var productTable = await CreateProductTableAsync(products);

                var notificationText = $"Trang [bold yellow]{currentPage}[/] / [bold yellow]{totalPages}[/]\n" +
                                       "[bold]Lệnh: [blue]'n'[/](Sau), [blue]'p'[/](Trước), [blue]'p.{số}'[/](Đến trang), [red]'exit'[/] hoặc chọn Menu[/]";

                _layout.Render(menuContent, productTable, new Markup(notificationText));

                Console.Write("\n> Nhập lệnh: ");
                string choice = Console.ReadLine()?.ToLower().Trim() ?? "";

                switch (choice)
                {
                    case "exit": return;
                    case "n": if (currentPage < totalPages) currentPage++; break;
                    case "p": if (currentPage > 1) currentPage--; break;
                    case "1": currentSortBy = "default"; currentPage = 1; break;
                    case "2": currentSortBy = "price_desc"; currentPage = 1; break;
                    case "3": currentSortBy = "price_asc"; currentPage = 1; break;
                    case "4": await HandleSearchAsync(); break;
                    case "5": await HandleCategoryFilterAsync(); break;
                    default:
                        if (choice.StartsWith("p.") && choice.Length > 2)
                        {
                            if (int.TryParse(choice.Substring(2), out int targetPage) && targetPage >= 1 && targetPage <= totalPages)
                            {
                                currentPage = targetPage;
                            }
                        }
                        break;
                }
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

        private async Task<Table> CreateProductTableAsync(List<Product> products)
        {
            var table = new Table().Expand().Border(TableBorder.Rounded);
            table.Title = new TableTitle("DANH SÁCH SẢN PHẨM");

            // Add columns to the table
            table.AddColumn(new TableColumn("[yellow]ID[/]").Centered());
            table.AddColumn(new TableColumn("[yellow]Tên sản phẩm[/]"));
            table.AddColumn(new TableColumn("[yellow]Thương hiệu[/]"));
            table.AddColumn(new TableColumn("[yellow]Phong cách[/]")); // <-- New column
            table.AddColumn(new TableColumn("[yellow]Giá[/]") { Alignment = Justify.Right });

            if (!products.Any())
            {
                table.AddRow(new Markup("[red]Không có sản phẩm nào để hiển thị.[/]").Centered());
                return table;
            }

            foreach (var product in products)
            {
                var (discountedPrice, promoCode) = await _promotionService.CalculateDiscountedPriceAsync(product);

                string priceDisplay;
                if (discountedPrice.HasValue)
                {
                    priceDisplay = $"[strikethrough dim red]{product.Price:N0}[/] [bold green]{discountedPrice.Value:N0} VNĐ[/]\n[italic dim]Áp dụng: {promoCode}[/]";
                }
                else
                {
                    priceDisplay = $"[green]{product.Price:N0} VNĐ[/]\n ";
                }

                var brand = product.ProductCategories?.Select(pc => pc.Category).FirstOrDefault(c => c.CategoryType == "Brand")?.Name ?? "N/A";

                var styleCategories = product.ProductCategories?
                                             .Where(pc => pc.Category.CategoryType == "Product")
                                             .Select(pc => pc.Category.Name)
                                             .ToList();
                var stylesDisplay = styleCategories != null && styleCategories.Any()
                                    ? string.Join("\n", styleCategories)
                                    : "N/A";

                // Add rows using the correct data type (Markup for formatted text)
                table.AddRow(
                    new Markup(product.ProductID.ToString()),
                    new Markup(Markup.Escape(product.Name)),
                    new Markup(Markup.Escape(brand)),
                    new Markup(Markup.Escape(stylesDisplay)),
                    new Markup(priceDisplay)
                );
            }
            return table;
        }

    }
}