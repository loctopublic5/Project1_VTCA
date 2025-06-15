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
        private readonly ConsoleLayout _layout;

        public ProductMenu(IProductService productService, ConsoleLayout layout)
        {
            _productService = productService;
            _layout = layout;
        }

        public async Task ShowAllProductsAsync()
        {
            int currentPage = 1;
            int totalPages = 1;
            const int PageSize = 10;

            while (true)
            {
                var (products, calculatedTotalPages) = await _productService.GetActiveProductsPaginatedAsync(currentPage, PageSize);
                totalPages = calculatedTotalPages > 0 ? calculatedTotalPages : 1;

                // Tạo Menu tĩnh bên trái
                var menuContent = new Markup("[dim]Đây là menu xem sản phẩm.\nSau này có thể thêm các bộ lọc ở đây.[/]");

                // Tạo View dạng Grid cho sản phẩm
                var productGrid = CreateProductGrid(products);

                // Tạo thông báo và hướng dẫn phân trang
                var notificationText = $"Trang [bold yellow]{currentPage}[/] / [bold yellow]{totalPages}[/]\n" +
                                       "[bold]Điều hướng: [blue]<=[/](Trang trước) | [blue]=>[/](Trang sau) | [red]ESC[/](Quay lại)[/]";
                var notification = new Markup(notificationText);

                // Vẽ lại toàn bộ layout
                _layout.Render(menuContent, productGrid, notification);

                // Chờ lệnh từ người dùng bằng ReadKey
                ConsoleKeyInfo keyInfo = Console.ReadKey(true);

                if (keyInfo.Key == ConsoleKey.Escape)
                {
                    break; // Thoát khỏi vòng lặp xem sản phẩm
                }
                else if (keyInfo.Key == ConsoleKey.RightArrow)
                {
                    if (currentPage < totalPages) currentPage++;
                }
                else if (keyInfo.Key == ConsoleKey.LeftArrow)
                {
                    if (currentPage > 1) currentPage--;
                }
            }
        }

        private Grid CreateProductGrid(List<Product> products)
        {
            var grid = new Grid();
            if (!products.Any())
            {
                return grid.AddRow(new Text("Không có sản phẩm nào để hiển thị."));
            }

            // Thêm số cột tương ứng với số sản phẩm (tối đa 10)
            for (int i = 0; i < products.Count; i++)
            {
                grid.AddColumn(new GridColumn().PadRight(2));
            }

            foreach (var product in products)
            {
                var brand = product.ProductCategories
                                   .Select(pc => pc.Category)
                                   .FirstOrDefault(c => c.CategoryType == "Brand")?.Name ?? "N/A";

                var productInfo = new Markup(
                    $"[bold yellow]ID: {product.ProductID}[/]\n" +
                    $"[bold]{Markup.Escape(product.Name)}[/]\n" +
                    $"[green]{product.Price:N0} VNĐ[/]\n" +
                    $"[dim]{brand}[/]"
                );

                var panel = new Panel(productInfo).Expand();
                grid.AddRow(panel); // Add each panel as a row
            }

            return grid;
        }

    }
}