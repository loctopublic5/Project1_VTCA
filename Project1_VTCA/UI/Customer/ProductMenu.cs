using Microsoft.EntityFrameworkCore;
using Project1_VTCA.Data;
using Project1_VTCA.Services.Interface;
using Project1_VTCA.UI.Customer.Interfaces;
using Project1_VTCA.UI.Draw;
using Project1_VTCA.Utils;
using Spectre.Console;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace Project1_VTCA.UI.Customer
{
    public class ProductMenu
    {
        private readonly IProductService _productService;
        private readonly IPromotionService _promotionService;
        private readonly ISessionService _sessionService;
        private readonly ICartService _cartService;
        private readonly ICheckoutMenu _checkoutMenu;
        private readonly ICartMenu _cartMenu;
        private readonly ConsoleLayout _layout;
        private List<Category> _productCategories;

        public ProductMenu(
            IProductService productService,
            IPromotionService promotionService,
            ISessionService sessionService,
            ICartService cartService,
            ICheckoutMenu checkoutMenu,
            ICartMenu cartMenu,
            ConsoleLayout layout)
        {
            _productService = productService;
            _promotionService = promotionService;
            _sessionService = sessionService;
            _cartService = cartService;
            _checkoutMenu = checkoutMenu;
            _cartMenu = cartMenu; // Gán
            _layout = layout;
            _productCategories = new List<Category>();
        }

        public async Task HandleViewProductDetailsAsync(int productId)
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
                    "1. Mua ngay \n" +
                    "2. Thêm vào giỏ hàng\n\n" +
                    "[red]3. Quay lại[/]"
                );

                var brand = product.ProductCategories?.Select(pc => pc.Category).FirstOrDefault(c => c.CategoryType == "Brand")?.Name ?? "N/A";
                var styles = string.Join(", ", product.ProductCategories?.Where(pc => pc.Category.CategoryType == "Product").Select(c => c.Category.Name) ?? Enumerable.Empty<string>());
                var priceDisplay = discountedPrice.HasValue
                    ? $"[strikethrough dim red]{product.Price:N0} VNĐ[/]  [bold green]{discountedPrice.Value:N0} VNĐ[/] ([italic]Tiết kiệm: {product.Price - discountedPrice.Value:N0} VNĐ[/])"
                    : $"[bold green]{product.Price:N0} VNĐ[/]";
                var sizes = string.Join(" | ", product.ProductSizes?.Where(s => (s.QuantityInStock ?? 0) > 0).Select(s => $"{s.Size} (SL: {s.QuantityInStock})") ?? Enumerable.Empty<string>());

                var detailsPanel = new Panel(
                    new Rows(
                        new FigletText(Markup.Escape(brand)).Color(Color.Orange1),
                        new Text(Markup.Escape(product.Name), new Style(decoration: Decoration.Bold | Decoration.Underline)).LeftJustified(),
                        new Text(""),
                        new Markup($"[bold]Phong cách:[/] {Markup.Escape(styles)}"),
                        new Markup($"[bold]Giá:[/] {priceDisplay}"),
                        new Rule("[yellow]Mô tả[/]").LeftJustified(),
                        new Padder(new Markup(Markup.Escape(product.Description ?? "")), new Padding(0, 0, 0, 1)),
                        new Rule("[yellow]Size còn hàng[/]").LeftJustified(),
                        new Text(Markup.Escape(sizes))
                    ))
                    .Header($"CHI TIẾT SẢN PHẨM - ID: {product.ProductID}")
                    .Expand();

                var notification = new Markup("[dim]Chọn một hành động từ menu bên trái.[/]");
                _layout.Render(actionMenu, detailsPanel, notification);

                Console.Write("\n> Nhập lựa chọn hành động: ");
                string choice = Console.ReadLine()?.Trim() ?? "";

                switch (choice)
                {
                    case "1":
                        await HandleBuyNowFlowAsync(product);
                        break;
                    case "2":
                        await HandleAddToCartFlowAsync(product);
                        break;
                    case "3":
                        return;
                }
            }
        }

        private async Task HandleBuyNowFlowAsync(Product product)
        {
            if (!_sessionService.IsLoggedIn)
            {
                AnsiConsole.MarkupLine("\n[red]Bạn cần đăng nhập để thực hiện chức năng này.[/]");
                Console.ReadKey();
                return;
            }

            AnsiConsole.Clear();
            Banner.Show();
            AnsiConsole.MarkupLine($"[bold underline]MUA NGAY SẢN PHẨM: {Markup.Escape(product.Name)}[/]");

            var availableSizes = product.ProductSizes?.Where(s => (s.QuantityInStock ?? 0) > 0).ToList();
            if (availableSizes == null || !availableSizes.Any())
            {
                AnsiConsole.MarkupLine("\n[red]Sản phẩm này hiện đã hết hàng.[/]");
                Console.ReadKey();
                return;
            }

            var selectedSize = AnsiConsole.Prompt(
                new SelectionPrompt<ProductSize>()
                    .Title($"Chọn [green]size[/]:")
                    .UseConverter(ps => $"{ps.Size} (Tồn kho: {ps.QuantityInStock})")
                    .AddChoices(availableSizes));

            var quantity = AnsiConsole.Prompt(
                new TextPrompt<int>($"Nhập [green]số lượng[/] cho size [yellow]{selectedSize.Size}[/]:")
                    .Validate(q => {
                        if (q <= 0) return ValidationResult.Error("[red]Số lượng phải lớn hơn 0.[/]");
                        if (q > 5) return ValidationResult.Error("[red]Chỉ được mua tối đa 5 sản phẩm mỗi lần.[/]");
                        if (q > (selectedSize.QuantityInStock ?? 0)) return ValidationResult.Error("[red]Số lượng vượt quá tồn kho.[/]");
                        return ValidationResult.Success();
                    }));

            var itemToCheckout = new List<CartItem>
            {
                new CartItem { Product = product, ProductID = product.ProductID, Size = selectedSize.Size, Quantity = quantity, UserID = _sessionService.CurrentUser.UserID }
            };

            // Bắt đầu luồng thanh toán
            await _checkoutMenu.StartCheckoutFlowAsync(itemToCheckout);
        }

        private async Task HandleAddToCartFlowAsync(Product product)
        {
            if (!_sessionService.IsLoggedIn)
            {
                AnsiConsole.MarkupLine("\n[red]Bạn cần đăng nhập để thực hiện chức năng này.[/]");
                Console.ReadKey();
                return;
            }

            AnsiConsole.Clear();
            Banner.Show();
            AnsiConsole.MarkupLine($"[bold underline]THÊM VÀO GIỎ HÀNG: {Markup.Escape(product.Name)}[/]");

            var availableSizes = product.ProductSizes?.Where(s => (s.QuantityInStock ?? 0) > 0).ToList();
            if (availableSizes == null || !availableSizes.Any())
            {
                AnsiConsole.MarkupLine("\n[red]Sản phẩm này hiện đã hết hàng.[/]");
                Console.ReadKey();
                return;
            }

            var selectedSize = AnsiConsole.Prompt(
                new SelectionPrompt<ProductSize>()
                    .Title($"Chọn [green]size[/]:")
                    .UseConverter(ps => $"{ps.Size} (Tồn kho: {ps.QuantityInStock})")
                    .AddChoices(availableSizes)
            );

            var quantity = AnsiConsole.Prompt(
                new TextPrompt<int>($"Nhập [green]số lượng[/] cho size [yellow]{selectedSize.Size}[/]:")
                    .Validate(q => {
                        if (q <= 0) return ValidationResult.Error("[red]Số lượng phải lớn hơn 0.[/]");
                        if (q > 5) return ValidationResult.Error("[red]Chỉ được mua tối đa 5 sản phẩm mỗi lần.[/]");
                        var stock = selectedSize.QuantityInStock ?? 0;
                        if (q > stock) return ValidationResult.Error($"[red]Số lượng tồn kho không đủ (chỉ còn {stock}).[/]");
                        return ValidationResult.Success();
                    }));

            var (discountedPrice, _) = await _promotionService.CalculateDiscountedPriceAsync(product);
            var finalPrice = discountedPrice ?? product.Price;

            var table = new Table().Border(TableBorder.None).HideHeaders().Expand();
            table.AddColumn("");
            table.AddColumn("");
            table.AddRow("[bold]Sản phẩm:[/]", Markup.Escape(product.Name));
            table.AddRow("[bold]Size đã chọn:[/]", $"[yellow]{selectedSize.Size}[/]");
            table.AddRow("[bold]Số lượng:[/]", $"[yellow]{quantity}[/]");
            table.AddRow("[bold]Tổng cộng:[/]", $"[green]{finalPrice * quantity:N0} VNĐ[/]");
            AnsiConsole.Write(new Panel(table).Header("XÁC NHẬN THÔNG TIN").Border(BoxBorder.Rounded));

            if (!AnsiConsole.Confirm("\n[bold]Xác nhận thêm vào giỏ hàng?[/]", defaultValue: true))
            {
                AnsiConsole.MarkupLine("[yellow]Đã hủy thao tác.[/]");
                Console.ReadKey();
                return;
            }

            var response = await _cartService.AddToCartAsync(_sessionService.CurrentUser.UserID, product.ProductID, selectedSize.Size, quantity);
            string color = response.IsSuccess ? "green" : "red";
            AnsiConsole.MarkupLine($"\n[{color}]{Markup.Escape(response.Message)}[/]");
            Console.ReadKey();
        }

        private async Task<Table> CreateProductTableAsync(List<Product> products)
        {
            var table = new Table().Expand().Border(TableBorder.HeavyHead);
            table.Title = new TableTitle("DANH SÁCH SẢN PHẨM");

            table.AddColumn(new TableColumn("[yellow]ID[/]").Alignment(Justify.Center));
            table.AddColumn(new TableColumn("[yellow]Tên sản phẩm[/]"));
            table.AddColumn(new TableColumn("[yellow]Thương hiệu[/]"));
            table.AddColumn(new TableColumn("[yellow]Danh mục chính[/]"));
            table.AddColumn(new TableColumn("[yellow]Giá Gốc[/]").Alignment(Justify.Right));
            table.AddColumn(new TableColumn("[yellow]Giá Mới[/]").Alignment(Justify.Right));

            if (!products.Any())
            {
                table.AddRow(new Text("Không có sản phẩm nào để hiển thị.", new Style(Color.Red))).Centered();
                return table;
            }

            foreach (var product in products)
            {
                var (discountedPrice, _) = await _promotionService.CalculateDiscountedPriceAsync(product);
                var displayCategory = _productService.GetDisplayCategory(product);
                var brand = product.ProductCategories?.Select(pc => pc.Category).FirstOrDefault(c => c.CategoryType == "Brand")?.Name ?? "N/A";

                table.AddRow(
                    new Markup(product.ProductID.ToString()),
                    new Markup(Markup.Escape(product.Name)),
                    new Markup(brand),
                    new Markup(Markup.Escape(displayCategory)),
                    new Markup($"[dim]{product.Price:N0} VNĐ[/]"),
                    discountedPrice.HasValue
                        ? new Markup($"[bold green]{discountedPrice.Value:N0} VNĐ[/]")
                        : new Markup("") // Để trống nếu không có khuyến mãi
                );
            }
            return table;
        }

        #region Other ProductMenu Methods
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
                var query = _productService.GetActiveProductsQuery();

                if (!string.IsNullOrEmpty(state.SearchTerm))
                {
                    var keywords = state.SearchTerm.ToLower().Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    foreach (var keyword in keywords)
                    {
                        query = query.Where(p => p.Name.ToLower().Contains(keyword));
                    }
                }

                if (state.CategoryIds != null && state.CategoryIds.Any())
                {
                    query = query.Where(p => p.ProductCategories.Any(pc => state.CategoryIds.Contains(pc.CategoryID)));
                }

                if (state.MinPrice.HasValue) query = query.Where(p => p.Price >= state.MinPrice.Value);
                if (state.MaxPrice.HasValue) query = query.Where(p => p.Price <= state.MaxPrice.Value);

                var (products, totalPages) = await _productService.GetPaginatedProductsAsync(query, state.CurrentPage, state.PageSize, state.SortBy);
                state.TotalPages = totalPages;

                var menuContent = CreateSideMenu();
                var productTable = await CreateProductTableAsync(products);
                var notificationText = $"Trang [bold yellow]{state.CurrentPage}[/] / [bold yellow]{state.TotalPages}[/]\n" +
                                       "Lệnh: [blue]'n'[/](Sau), [blue]'p'[/](Trước), [yellow]'id.{số}'[/], [red]'exit'[/] hoặc chọn Menu";
                _layout.Render(menuContent, productTable, new Markup($"[bold]{notificationText}[/]"));

                Console.Write("\n> Nhập lệnh: ");
                string choice = Console.ReadLine()?.ToLower().Trim() ?? "";

                if (await HandleChoice(choice, state) == false)
                {
                    return false;
                }
            }
        }

        private async Task<bool> HandleChoice(string choice, ProductDisplayState state)
        {
            if (choice.StartsWith("add.") || choice.StartsWith("buy."))
            {
                var parts = choice.Split('.');
                if (parts.Length == 2 && int.TryParse(parts[1], out int productId))
                {
                    var product = await _productService.GetProductByIdAsync(productId);
                    if (product == null)
                    {
                        AnsiConsole.MarkupLine("[red]Lỗi: Không tìm thấy sản phẩm với ID này.[/]");
                        Console.ReadKey();
                        return true;
                    }

                    if (parts[0] == "add")
                    {
                        await HandleAddToCartFlowAsync(product);
                    }
                    else // parts[0] == "buy"
                    {
                        await HandleBuyNowFlowAsync(product);
                    }
                }
                else
                {
                    AnsiConsole.MarkupLine("[red]Lỗi: Cú pháp lệnh không hợp lệ.[/]");
                    Console.ReadKey();
                }
                return true; // Quay lại vòng lặp để làm mới danh sách
            }

            if (choice.StartsWith("id."))
            {
                if (int.TryParse(choice.AsSpan(3), out int productId))
                {
                    await HandleViewProductDetailsAsync(productId);
                }
                return true;
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
                case "7":
                    if (!_sessionService.IsLoggedIn)
                    {
                        AnsiConsole.MarkupLine("\n[red]Bạn cần đăng nhập để sử dụng chức năng giỏ hàng. Vui lòng quay lại và chọn 'Đăng nhập' hoặc 'Đăng ký'.[/]");
                        Console.ReadKey();
                    }
                    else
                    {
                        await _cartMenu.ShowAsync();
                    }
                    break;
                default:
                    if (choice.StartsWith("p.") && int.TryParse(choice.AsSpan(2), out int targetPage) && targetPage >= 1 && targetPage <= state.TotalPages)
                    {
                        state.CurrentPage = targetPage;
                    }
                    break;
            }
            return true;
        }

        private Markup CreateSideMenu()
        {
            return new Markup(
                "[bold yellow underline]HÀNH ĐỘNG NHANH[/]\n" +
                "[blue]add.{id}[/] - Thêm vào giỏ hàng\n" +
                "[blue]buy.{id}[/] - Mua ngay sản phẩm\n" +
                "[blue]id.{id}[/]  - Xem chi tiết sản phẩm\n\n" +
                "[bold yellow underline]LỌC & SẮP XẾP[/]\n" +
                "1. Sắp xếp (Mặc định)\n" +
                "2. Sắp xếp (Giá: Cao-Thấp)\n" +
                "3. Sắp xếp (Giá: Thấp-Cao)\n" +
                "4. Tìm kiếm theo tên\n" +
                "5. Lọc theo danh mục\n" +
                "6. Lọc theo giá\n" +
                "0. Quay về mặc định\n\n" +
                "[bold yellow underline]GIỎ HÀNG[/]\n"+
                "7. [cyan]Xem/Quản lý Giỏ hàng[/]\n"+
               "[bold yellow underline]ĐIỀU HƯỚNG[/]\n" +
                "[dim]n - Trang sau\n" +
                "p - Trang trước\n" +
                "p.{số} - Đến trang chỉ định[/]\n" +
                "[red]exit[/] - Thoát"
            );
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
                    return (null, null);
            }
        }



        private async Task<Table> CreateProductTableAsync(List<Product> products, bool includeMainStyle = false)
        {
            var table = new Table().Expand().Border(TableBorder.HeavyHead);
            table.Title = new TableTitle("DANH SÁCH SẢN PHẨM");

            table.AddColumn(new TableColumn("[yellow]ID[/]") { Alignment = Justify.Center });
            table.AddColumn(new TableColumn("[yellow]Tên sản phẩm[/]"));
            table.AddColumn(new TableColumn("[yellow]Thương hiệu[/]"));
            if (includeMainStyle)
            {
                table.AddColumn(new TableColumn("[yellow]Phong cách chính[/]"));
            }
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

                if (includeMainStyle)
                {
                    var mainStyle = product.ProductCategories?
                                         .Select(pc => pc.Category)
                                         .FirstOrDefault(c => c.CategoryType == "Product")?.Name ?? "N/A";

                    table.AddRow(
                        new Markup(product.ProductID.ToString()),
                        new Markup(Markup.Escape(product.Name)),
                        new Markup(brand),
                        new Markup(mainStyle),
                        new Markup(priceDisplay)
                    );
                }
                else
                {
                    table.AddRow(
                        new Markup(product.ProductID.ToString()),
                        new Markup(Markup.Escape(product.Name)),
                        new Markup(brand),
                        new Markup(priceDisplay)
                    );
                }
            }
            return table;
        }


        private async Task<string> HandleSearchPrompt()
        {
            Console.Write("\n> Nhập từ khóa tìm kiếm (hoặc 'exit'): ");
            string searchTerm = Console.ReadLine()?.Trim() ?? "";
            return searchTerm.Equals("exit", StringComparison.OrdinalIgnoreCase) ? null : searchTerm;
        }
        #endregion
    }
}