using Project1_VTCA.Data;
using Project1_VTCA.Services;
using Spectre.Console;
using Spectre.Console.Rendering;
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

        // --- PHƯƠNG THỨC ĐÃ ĐƯỢC CẢI TIẾN ---
        private Grid CreateProductGrid(List<Product> products)
        {
            var grid = new Grid().Expand();
            if (!products.Any())
            {
                return grid.AddRow(new Text("\nKhông có sản phẩm nào.", new Style(Color.Yellow)).Centered());
            }

            // Định nghĩa một grid luôn có 5 cột
            grid.AddColumns(new GridColumn(), new GridColumn(), new GridColumn(), new GridColumn(), new GridColumn());

            // Tạo danh sách các Panel cho từng sản phẩm
            var panels = products.Select(product => {
                var brand = product.ProductCategories?
                                   .Select(pc => pc.Category)
                                   .FirstOrDefault(c => c.CategoryType == "Brand")?.Name ?? "N/A";
                var productInfo = new Markup(
                    $"[bold yellow]ID: {product.ProductID}[/]\n" +
                    $"[bold]{Markup.Escape(product.Name)}[/]\n" +
                    $"[green]{product.Price:N0} VNĐ[/]\n" +
                    $"[dim]{brand}[/]");
                return new Panel(productInfo).Border(BoxBorder.Rounded).Expand();
            }).ToList();

            // Lặp và thêm các sản phẩm vào grid, mỗi hàng 5 sản phẩm
            for (int i = 0; i < panels.Count; i += 5)
            {
                var rowItems = panels.Skip(i).Take(5).Cast<IRenderable>().ToList();
                // Nếu hàng cuối không đủ 5, thêm các ô trống để giữ đúng cấu trúc
                while (rowItems.Count < 5)
                {
                    rowItems.Add(new Text(""));
                }
                grid.AddRow(rowItems.ToArray());
            }

            return grid;
        }
    }
}