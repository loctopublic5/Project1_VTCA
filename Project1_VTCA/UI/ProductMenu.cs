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
                // Nội dung cho khung Menu bên trái
                var menuContent = new Markup(
                    "[bold yellow underline]SẮP XẾP[/]\n" +
                    "1. Mặc định (Mới nhất)\n" +
                    "2. Giá: Cao đến thấp\n" +
                    "3. Giá: Thấp đến cao\n\n" +
                    "[bold yellow underline]LỌC[/]\n" +
                    "[dim](Các bộ lọc sẽ ở đây sau)[/]\n\n" +
                    "[dim]Nhập lựa chọn và nhấn Enter.[/]"
                );

                var (products, totalPages) = await _productService.GetActiveProductsPaginatedAsync(currentPage, PageSize, currentSortBy);
                totalPages = totalPages > 0 ? totalPages : 1;

                // Thay vì tạo Grid, chúng ta tạo Table
                var productTable = await CreateProductTableAsync(products);

                var notificationText = $"Trang [bold yellow]{currentPage}[/] / [bold yellow]{totalPages}[/]\n" +
                                       "[bold]Điều hướng: Gõ [blue]'n'[/](Sau), [blue]'p'[/](Trước), [red]'exit'[/] hoặc một lựa chọn Menu[/]";

                _layout.Render(menuContent, productTable, new Markup(notificationText));

                Console.Write("\n> Nhập lệnh: ");
                string choice = Console.ReadLine()?.ToLower().Trim() ?? "";

                switch (choice)
                {
                    case "exit": return;
                    case "n": if (currentPage < totalPages) currentPage++; break;
                    case "p": if (currentPage > 1) currentPage--; break;
                    case "1":
                        currentSortBy = "default";
                        currentPage = 1;
                        break;
                    case "2":
                        currentSortBy = "price_desc";
                        currentPage = 1;
                        break;
                    case "3":
                        currentSortBy = "price_asc";
                        currentPage = 1;
                        break;
                }
            }
        }

        // Phương thức này được viết lại để tạo Table và xử lý hiển thị giá
        private async Task<Table> CreateProductTableAsync(List<Product> products)
        {
            var table = new Table().Expand().Border(TableBorder.Rounded);
            table.Title = new TableTitle("DANH SÁCH SẢN PHẨM");

            // Thêm các cột cho bảng
            table.AddColumn(new TableColumn("[yellow]ID[/]").Centered());
            table.AddColumn(new TableColumn("[yellow]Tên sản phẩm[/]"));
            table.AddColumn(new TableColumn("[yellow]Giá[/]").RightAligned());
            table.AddColumn(new TableColumn("[yellow]Thương hiệu[/]"));

            if (!products.Any())
            {
                table.AddRow(new Text("Không có sản phẩm nào để hiển thị.", new Style(Color.Red))).Centered();
                return table;
            }

            foreach (var product in products)
            {
                // Tính giá khuyến mãi cho từng sản phẩm
                var (discountedPrice, promoCode) = await _promotionService.CalculateDiscountedPriceAsync(product);

                string priceDisplay;
                // --- PHẦN LOGIC HIỂN THỊ GIÁ NÂNG CAO ---
                if (discountedPrice.HasValue)
                {
                    // Nếu có giảm giá, hiển thị giá cũ gạch đi và giá mới nổi bật
                    priceDisplay = $"[strikethrough dim red]{product.Price:N0}[/] [bold green]{discountedPrice.Value:N0} VNĐ[/]\n[italic dim]Áp dụng: {promoCode}[/]";
                }
                else
                {
                    // Nếu không, hiển thị giá gốc bình thường
                    priceDisplay = $"[green]{product.Price:N0} VNĐ[/]";
                }

                var brand = product.ProductCategories?.Select(pc => pc.Category).FirstOrDefault(c => c.CategoryType == "Brand")?.Name ?? "N/A";

                // Thêm một hàng mới cho mỗi sản phẩm
                table.AddRow(
                new Markup(product.ProductID.ToString()),
                new Markup(Markup.Escape(product.Name)),
                new Markup(priceDisplay),
                new Markup(Markup.Escape(brand))
                    );
            }
            return table;
        }
    }
}