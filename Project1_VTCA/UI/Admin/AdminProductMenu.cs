using Microsoft.Extensions.DependencyInjection;
using Project1_VTCA.Data;
using Project1_VTCA.Services.Interface;
using Project1_VTCA.UI.Admin.Interface;
using Project1_VTCA.UI.Customer;
using Project1_VTCA.UI.Draw;
using Project1_VTCA.UI.Customer.Interfaces;
using Project1_VTCA.Utils;
using Spectre.Console;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project1_VTCA.UI.Admin
{
    public class AdminProductMenu : IAdminProductMenu
    {
        private readonly IProductService _productService;
        private readonly IServiceProvider _serviceProvider;
        private readonly ConsoleLayout _layout;

      
        private class ProductListViewState
        {
            public int CurrentPage { get; set; } = 1;
            public int PageSize { get; set; } = 10;
            public int TotalPages { get; set; }
            public string SortBy { get; set; } = "default";
        }

        public AdminProductMenu(IProductService productService, IServiceProvider serviceProvider, ConsoleLayout layout)
        {
            _productService = productService;
            _serviceProvider = serviceProvider;
            _layout = layout;
        }

        public async Task ShowAsync()
        {
            var state = new ProductListViewState();
            while (true)
            {
               
                var query = _productService.GetActiveProductsQuery();
                var (products, totalPages) = await _productService.GetPaginatedProductsAsync(query, state.CurrentPage, state.PageSize, "default");
                state.TotalPages = totalPages;
                if (state.CurrentPage > state.TotalPages && state.TotalPages > 0)
                {
                    state.CurrentPage = state.TotalPages;
                }


                var menuContent = CreateMainMenu();
                var viewContent = await CreateActiveProductTableAsync(products);
                var notificationContent = new Markup($"Trang [bold yellow]{state.CurrentPage}[/] / [bold yellow]{state.TotalPages}[/]. " +
                                                     "[dim]Chọn chức năng hoặc dùng lệnh nhanh (n, p, p.{{số}}, id.{{id}}).[/]");

                _layout.Render(menuContent, viewContent, notificationContent);

                var choice = AnsiConsole.Ask<string>("\n> Nhập lựa chọn:").ToLower();
                if (await HandleMainMenuCommand(choice, state) == false)
                {
                    return; 
                }
            }
        }

        private async Task<bool> HandleMainMenuCommand(string choice, ProductListViewState state)
        {
            if (choice.StartsWith("id."))
            {
                if (choice.StartsWith("id."))
                {
                    if (int.TryParse(choice.AsSpan(3), out int productId))
                    {
                        await ShowAdminProductDetailsAsync(productId);
                    }
                    return true;
                }
            }

            if (choice.StartsWith("p."))
            {
                if (int.TryParse(choice.AsSpan(2), out int page) && page > 0 && page <= state.TotalPages) state.CurrentPage = page;
                return true;
            }

            switch (choice)
            {
                case "1": await HandleAddStockAsync(); break;
                case "2": await HandleAddNewProductAsync(); break;
                case "3": await HandleUpdateStockAsync(); break;
                case "4": await HandleSoftDeleteProductAsync(); break;
                case "5": await HandleViewInactiveProductsAsync(); break;
                case "s1": state.SortBy = "stock_desc"; state.CurrentPage = 1; break;
                case "s2": state.SortBy = "stock_asc"; state.CurrentPage = 1; break;
                case "n": if (state.CurrentPage < state.TotalPages) state.CurrentPage++; break;
                case "p": if (state.CurrentPage > 1) state.CurrentPage--; break;
                case "0": return false;
            }
            return true;
        }

        #region Main UI Creation

        private Markup CreateMainMenu()
        {
            return new Markup(
                "[bold blue underline]QUẢN LÝ SẢN PHẨM[/]\n\n" +
                "[bold]Chức năng Kho:[/]\n" +
                " 1. Thêm số lượng hàng\n" +
                " 2. Thêm sản phẩm mới\n" +
                " 3. Cập nhật kho (Ghi đè)\n\n" +
                "[bold]Quản lý Sản phẩm:[/]\n" +
                " 4. Gỡ sản phẩm khỏi kệ\n" +
                " 5. Xem sản phẩm đã rời kệ\n\n" +
                " [red]0. Quay lại[/]"
            );
        }

        private async Task<Table> CreateActiveProductTableAsync(List<Product> products)
        {
            var table = new Table().Expand().Border(TableBorder.Rounded);
            table.Title = new TableTitle("[yellow]DANH SÁCH SẢN PHẨM ĐANG HOẠT ĐỘNG[/]");
            table.AddColumn("ID");
            table.AddColumn("Tên sản phẩm");
            table.AddColumn("Danh mục chính");
            table.AddColumn("Tổng tồn kho");
            table.AddColumn("Giá (VNĐ)");

            if (!products.Any())
            {
                table.AddRow("[grey]Không có sản phẩm nào.[/]", "", "", "", "");
                return table;
            }

            var promotionService = _serviceProvider.GetRequiredService<IPromotionService>();
            foreach (var product in products)
            {
                var (discountedPrice, _) = await promotionService.CalculateDiscountedPriceAsync(product);
                var priceDisplay = discountedPrice.HasValue
                    ? $"[strikethrough dim red]{product.Price:N0}[/] [bold green]{discountedPrice.Value:N0}[/]"
                    : $"[green]{product.Price:N0}[/]";
                var calculatedStock = product.ProductSizes?.Sum(ps => ps.QuantityInStock) ?? 0;

                table.AddRow(
                    new Markup(product.ProductID.ToString()),
                    new Markup(Markup.Escape(product.Name)),
                    new Markup(Markup.Escape(_productService.GetDisplayCategory(product))),
                    new Markup(calculatedStock.ToString()),
                    new Markup($"[green]{product.Price:N0}[/]")
                );
            }
            return table;
        }

        #endregion

        #region Feature Handlers

        private async Task HandleAddStockAsync(Product? preselectedProduct = null)
        {
            await _layout.RenderFormLayoutAsync("THÊM SỐ LƯỢNG HÀNG (CỘNG DỒN)", async () =>
            {
                Product? product = preselectedProduct;
                if (product == null)
                {
                    var productIdStr = AnsiConsole.Ask<string>("\nNhập [green]ID Sản phẩm[/] cần thêm hàng (hoặc '[red]exit[/]'):");
                    if (productIdStr.Equals("exit", StringComparison.OrdinalIgnoreCase)) return;

                    if (!int.TryParse(productIdStr, out int id) || (product = await _productService.GetProductByIdIncludingInactiveAsync(id)) == null)
                    {
                        AnsiConsole.MarkupLine("[red]Lỗi: Không tìm thấy sản phẩm với ID này.[/]");
                        Console.ReadKey();
                        return;
                    }
                }

                await ShowAdminProductDetailsAsync(product.ProductID);
                AnsiConsole.Write(new Rule("[yellow]Thao tác thêm kho[/]").Centered());

                var productSizes = await _productService.GetProductSizesAsync(product.ProductID);
                var validSizes = _productService.GetValidSizesForGender(product.GenderApplicability);
                var filteredSizes = productSizes.Where(ps => validSizes.Contains(ps.Size)).ToList();

                if (!filteredSizes.Any())
                {
                    AnsiConsole.MarkupLine("[red]Sản phẩm này không có size hợp lệ để thêm hàng.[/]");
                    Console.ReadKey();
                    return;
                }

                var sizesToUpdate = AnsiConsole.Prompt(
                    new MultiSelectionPrompt<ProductSize>()
                        .Title("Chọn các [green]size[/] cần thêm hàng:")
                        .PageSize(10).UseConverter(ps => $"Size {ps.Size} (Hiện có: {ps.QuantityInStock})")
                        .AddChoices(filteredSizes)
                );

                if (!sizesToUpdate.Any()) return;

                var quantityToAdd = AnsiConsole.Ask<int>("Nhập [green]số lượng cần cộng thêm[/] cho mỗi size đã chọn:");
                if (quantityToAdd <= 0)
                {
                    AnsiConsole.MarkupLine("[red]Lỗi: Số lượng thêm vào phải lớn hơn 0.[/]");
                    Console.ReadKey();
                    return;
                }

                if (AnsiConsole.Confirm($"Bạn có chắc muốn cộng thêm [yellow]{quantityToAdd}[/] sản phẩm cho [yellow]{sizesToUpdate.Count}[/] size đã chọn không?"))
                {
                    var sizeIds = sizesToUpdate.Select(s => s.Size).ToList();
                    var response = await _productService.AddStockAsync(product.ProductID, sizeIds, quantityToAdd);
                    AnsiConsole.MarkupLine($"\n[{(response.IsSuccess ? "green" : "red")}]{Markup.Escape(response.Message)}[/]");
                    Console.ReadKey();
                }
            });
        }

        
        private async Task HandleAddNewProductAsync()
        {
            await _layout.RenderFormLayoutAsync("THÊM SẢN PHẨM MỚI", async () =>
            {
                var brands = (await _productService.GetCategoriesByTypeAsync("Brand"))
                                .Where(c => c.ParentID != null).ToList();
                var styles = (await _productService.GetCategoriesByTypeAsync("Product"))
                                .Where(c => c.ParentID != null).ToList();

                var selectedBrand = AnsiConsole.Prompt(new SelectionPrompt<Category>().Title("Bước 1: Chọn [green]Thương hiệu[/]:").AddChoices(brands).UseConverter(c => c.Name));
                var selectedStyles = AnsiConsole.Prompt(new MultiSelectionPrompt<Category>().Title("Bước 2: Chọn [green]Danh mục/Phong cách[/]:").AddChoices(styles).UseConverter(c => c.Name));
                if (!selectedStyles.Any()) { AnsiConsole.MarkupLine("[red]Bạn phải chọn ít nhất một danh mục.[/]"); Console.ReadKey(); return; }

                var name = AnsiConsole.Ask<string>("Bước 3: Nhập [green]Tên sản phẩm[/]:");
                var description = AnsiConsole.Ask<string>("Bước 4: Nhập [green]Mô tả[/]:");
                var price = AnsiConsole.Ask<decimal>("Bước 5: Nhập [green]Đơn giá[/] (VNĐ):");
                var gender = AnsiConsole.Prompt(new SelectionPrompt<string>().Title("Bước 6: Chọn [green]Giới tính[/] áp dụng:").AddChoices("Unisex", "Male", "Female"));

                var newProduct = new Product { Name = name, Description = description, Price = price, GenderApplicability = gender, IsActive = true };
                var allCategoryIds = new List<int> { selectedBrand.CategoryID }.Concat(selectedStyles.Select(s => s.CategoryID)).ToList();

                if (AnsiConsole.Confirm("\nXác nhận tạo sản phẩm với các thông tin trên?"))
                {
                    var createdProduct = _productService.AddNewProductAsync(newProduct, allCategoryIds).Result;
                    if (createdProduct != null)
                    {
                        AnsiConsole.MarkupLine($"[green]Đã tạo thành công sản phẩm: {Markup.Escape(createdProduct.Name)} (ID: {createdProduct.ProductID})[/]");
                        if (AnsiConsole.Confirm("[cyan]Bạn có muốn tiếp tục thêm số lượng tồn kho cho sản phẩm này không?[/]"))
                        {
                           
                            HandleAddStockAsync(createdProduct).Wait();
                        }
                    }
                    else
                    {
                        AnsiConsole.MarkupLine("[red]Lỗi: Không thể tạo sản phẩm mới.[/]");
                        Console.ReadKey();
                    }
                }
            });
        }



        private async Task HandleUpdateStockAsync()
        {
            var productId = AnsiConsole.Ask<int>("Nhập [green]ID Sản phẩm[/] cần cập nhật kho (nhập 0 để thoát):");
            if (productId == 0) return;

            var product = await _productService.GetProductByIdAsync(productId);

            if (product == null) { AnsiConsole.MarkupLine("[red]Lỗi: Không tìm thấy sản phẩm.[/]"); Console.ReadKey(); return; }

            AnsiConsole.MarkupLine($"\nSản phẩm đã chọn: [yellow]{Markup.Escape(product.Name)}[/] (Giới tính: {product.GenderApplicability})");

            var allProductSizes = await _productService.GetProductSizesAsync(product.ProductID);
            var validSizes = _productService.GetValidSizesForGender(product.GenderApplicability);
            var filteredSizes = allProductSizes.Where(ps => validSizes.Contains(ps.Size)).ToList();

            if (!filteredSizes.Any())
            {
                AnsiConsole.MarkupLine("[red]Sản phẩm này không có size hợp lệ để cập nhật hàng theo giới tính đã chọn.[/]");
                Console.ReadKey();
                return;
            }

            var sizesToUpdate = AnsiConsole.Prompt(
                new MultiSelectionPrompt<ProductSize>()
                    .Title($"Chọn các [green]size[/] cần cập nhật số lượng:")
                    .PageSize(10).UseConverter(ps => $"Size {ps.Size} (Hiện có: {ps.QuantityInStock})")
                    .AddChoices(filteredSizes)
            );


            if (!sizesToUpdate.Any()) return;

            var newQuantity = AnsiConsole.Ask<int>("Nhập [green]số lượng tồn kho MỚI[/] để ghi đè:");
            if (newQuantity < 0) { AnsiConsole.MarkupLine("[red]Số lượng không thể là số âm.[/]"); Console.ReadKey(); return; }

            if (AnsiConsole.Confirm($"Bạn có chắc muốn cập nhật tồn kho thành [yellow]{newQuantity}[/] cho [yellow]{sizesToUpdate.Count}[/] size đã chọn không?"))
            {
                var sizeIds = sizesToUpdate.Select(s => s.Size).ToList();
                var response = await _productService.UpdateStockAsync(productId, sizeIds, newQuantity);
                AnsiConsole.MarkupLine($"\n[{(response.IsSuccess ? "green" : "red")}]{Markup.Escape(response.Message)}[/]");
                Console.ReadKey();
            }
        }

        private async Task HandleSoftDeleteProductAsync()
        {
            var productId = AnsiConsole.Ask<int>("Nhập [green]ID Sản phẩm[/] cần gỡ khỏi kệ (nhập 0 để thoát):");
            if (productId == 0) return;

            var product = await _productService.GetProductByIdAsync(productId);
            if (product == null) { AnsiConsole.MarkupLine("[red]Lỗi: Không tìm thấy sản phẩm đang hoạt động với ID này.[/]"); Console.ReadKey(); return; }

            AnsiConsole.MarkupLine($"Bạn đã chọn sản phẩm: [yellow]{Markup.Escape(product.Name)}[/]");
            if (AnsiConsole.Confirm("[bold red]Bạn có chắc chắn muốn gỡ sản phẩm này khỏi kệ không?[/]\n[dim](Sản phẩm sẽ không thể được mua nhưng vẫn lưu trong lịch sử giao dịch)[/]"))
            {
                var response = await _productService.SoftDeleteProductAsync(productId);
                AnsiConsole.MarkupLine($"\n[{(response.IsSuccess ? "green" : "red")}]{Markup.Escape(response.Message)}[/]");
                Console.ReadKey();
            }
        }

        private async Task HandleViewInactiveProductsAsync()
        {
            var state = new ProductListViewState();
            while (true)
            {
                var (products, totalPages) = await _productService.GetInactiveProductsAsync(state.CurrentPage, state.PageSize);
                state.TotalPages = totalPages;

                var menuContent = new Markup("[bold grey]SẢN PHẨM ĐÃ GỠ[/]\n\n[red]0. Quay lại[/]");
                var viewContent = CreateInactiveProductTable(products);
                var notificationContent = new Markup($"Trang [bold yellow]{state.CurrentPage}[/] / [bold yellow]{state.TotalPages}[/]. Lệnh: n, p, p.{{số}}, id.{{id}}");

                _layout.Render(menuContent, viewContent, notificationContent);

                var choice = AnsiConsole.Ask<string>("\n> Nhập lựa chọn:").ToLower();

                if (choice == "0") break;

                if (choice.StartsWith("id."))
                {
                    if (int.TryParse(choice.AsSpan(3), out int productId))
                    {
                        await HandleViewInactiveProductDetailsAsync(productId);
                    }
                    continue;
                }

                if (choice.StartsWith("p."))
                {
                    if (int.TryParse(choice.AsSpan(2), out int page) && page > 0 && page <= state.TotalPages) state.CurrentPage = page;
                    continue;
                }

                switch (choice)
                {
                    case "n": if (state.CurrentPage < totalPages) state.CurrentPage++; break;
                    case "p": if (state.CurrentPage > 1) state.CurrentPage--; break;
                }
            }
        }

        private async Task HandleViewInactiveProductDetailsAsync(int productId)
        {
            
            var product = await _productService.GetProductByIdIncludingInactiveAsync(productId);

            if (product == null)
            {
                AnsiConsole.MarkupLine("[red]Lỗi: Không tìm thấy sản phẩm với ID này.[/]");
            }
            else
            {
                await ShowAdminProductDetailsAsync(productId);
            }
            Console.ReadKey();
        }

        private async Task ShowAdminProductDetailsAsync(int productId)
        {
            var product = await _productService.GetProductByIdIncludingInactiveAsync(productId);
            if (product == null)
            {
                AnsiConsole.MarkupLine("[red]Lỗi: Không tìm thấy sản phẩm với ID này.[/]");
                Console.ReadKey();
                return;
            }

            AnsiConsole.Clear();

            var infoGrid = new Grid()
                .AddColumn().AddColumn()
                .AddRow(new Markup("[bold]Tên sản phẩm:[/]"), new Markup(Markup.Escape(product.Name)))
                .AddRow(new Markup("[bold]Thương hiệu:[/]"), new Markup(Markup.Escape(product.ProductCategories?.Select(pc => pc.Category).FirstOrDefault(c => c.CategoryType == "Brand")?.Name ?? "N/A")))
                .AddRow(new Markup("[bold]Danh mục:[/]"), new Markup(Markup.Escape(_productService.GetDisplayCategory(product))))
                .AddRow(new Markup("[bold]Giá gốc:[/]"), new Markup($"[yellow]{product.Price:N0} VNĐ[/]"))
                .AddRow(new Markup("[bold]Giới tính áp dụng:[/]"), new Markup(Markup.Escape(product.GenderApplicability ?? "Không xác định")))
                .AddRow(new Markup("[bold]Trạng thái:[/]"), product.IsActive ? new Markup("[green]Available on shelf[/]") : new Markup("[red]Remove from shelf[/]"));

            var layoutRows = new Rows(
                infoGrid,
                new Rule("[yellow]Mô tả[/]").LeftJustified(),
                new Padder(new Markup(Markup.Escape(product.Description ?? "Không có mô tả.")), new Padding(0, 0, 0, 1))
            );

            var infoPanel = new Panel(layoutRows)
                .Header($"CHI TIẾT SẢN PHẨM - ID: {product.ProductID}")
                .Expand();
            AnsiConsole.Write(infoPanel);

            var stockTable = new Table().Expand().Border(TableBorder.Rounded);
            stockTable.Title = new TableTitle("Tồn kho chi tiết theo Size");
            stockTable.AddColumn("Size");
            stockTable.AddColumn("Số lượng trong kho");

            var sizes = await _productService.GetProductSizesAsync(productId);
            foreach (var size in sizes)
            {
                stockTable.AddRow(new Markup(size.Size.ToString()), new Markup(size.QuantityInStock.ToString()));
            }
            AnsiConsole.Write(stockTable);

            AnsiConsole.MarkupLine("\n[dim] Nhấn phím bất kỳ để tiếp tục.[/]");
            Console.ReadKey();
        }

        private Table CreateInactiveProductTable(List<Product> products)
        {
            var table = new Table().Expand().Border(TableBorder.Rounded);
            table.Title = new TableTitle("[grey]Sản phẩm đã rời kệ[/]");
            table.AddColumn("ID");
            table.AddColumn("Tên sản phẩm");
            table.AddColumn("Tổng tồn kho");

            if (!products.Any())
            {
                table.AddRow("[grey]Không có sản phẩm nào đã bị gỡ.[/]", "", "");
                return table;
            }

            foreach (var product in products)
            {
                var calculatedStock = product.ProductSizes?.Sum(ps => ps.QuantityInStock ?? 0) ?? 0;

                table.AddRow(
                    new Markup(product.ProductID.ToString()),
                    new Markup(Markup.Escape(product.Name)),
                    new Markup(calculatedStock.ToString())
                );
            }
            return table;
        }
        #endregion
    }
}