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
            const int PageSize = 10;
            var menuContent = new Markup("[dim]Đây là menu xem sản phẩm.\nSẽ có các bộ lọc ở đây sau.[/]");

            while (true)
            {
                var (products, totalPages) = await _productService.GetActiveProductsPaginatedAsync(currentPage, PageSize);
                totalPages = totalPages > 0 ? totalPages : 1;

                var productGrid = CreateProductGrid(products);
                var notificationText = $"Trang [bold yellow]{currentPage}[/] / [bold yellow]{totalPages}[/]\n" +
                                       "[bold]Điều hướng: [blue]<=[/](Trước) | [blue]=>[/](Sau) | [red]ESC[/](Quay lại)[/]";

                _layout.Render(menuContent, productGrid, new Markup(notificationText));

                ConsoleKeyInfo keyInfo = Console.ReadKey(true);
                if (keyInfo.Key == ConsoleKey.Escape) break;
                if (keyInfo.Key == ConsoleKey.RightArrow && currentPage < totalPages) currentPage++;
                if (keyInfo.Key == ConsoleKey.LeftArrow && currentPage > 1) currentPage--;
            }
        }

        private Grid CreateProductGrid(List<Product> products)
        {
            var grid = new Grid();
            if (!products.Any())
                return grid.AddRow(new Text("\nKhông có sản phẩm nào.", new Style(Color.Yellow)).Centered());

            foreach (var _ in products)
                grid.AddColumn(new GridColumn().PadRight(2));

            foreach (var product in products)
            {
                var brand = product.ProductCategories?.Select(pc => pc.Category).FirstOrDefault(c => c.CategoryType == "Brand")?.Name ?? "N/A";
                var productInfo = new Markup(
                    $"[bold yellow]ID: {product.ProductID}[/]\n" +
                    $"[bold]{Markup.Escape(product.Name)}[/]\n" +
                    $"[green]{product.Price:N0} VNĐ[/]\n" +
                    $"[dim]{brand}[/]");
                var panel = new Panel(productInfo).Expand();
                grid.AddRow(panel); // Add each panel as a separate row
            }

            return grid;
        }

    }
}