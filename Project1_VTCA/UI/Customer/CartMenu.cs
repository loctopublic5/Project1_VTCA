using Project1_VTCA.Data;
using Project1_VTCA.Services.Interface;
using Project1_VTCA.UI.Customer.Interfaces;
using Project1_VTCA.UI.Draw;
using Spectre.Console;
using Spectre.Console.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Project1_VTCA.UI.Customer
{
    public class CartMenu : ICartMenu
    {
        private readonly ICartService _cartService;
        private readonly ISessionService _sessionService;
        private readonly IPromotionService _promotionService;
        private readonly ConsoleLayout _layout;
        private readonly ProductMenu _productMenu;
        private readonly ICheckoutMenu _checkoutMenu; // THÊM: Để gọi luồng thanh toán

        private class CartState
        {
            public int CurrentPage { get; set; } = 1;
            public int PageSize { get; set; } = 5;
            public int TotalPages { get; set; } = 1;
        }

        // Cập nhật constructor
        public CartMenu(ICartService cartService, ISessionService sessionService, IPromotionService promotionService, ConsoleLayout layout, ProductMenu productMenu, ICheckoutMenu checkoutMenu)
        {
            _cartService = cartService;
            _sessionService = sessionService;
            _promotionService = promotionService;
            _layout = layout;
            _productMenu = productMenu;
            _checkoutMenu = checkoutMenu; // Gán service mới
        }

        public async Task ShowAsync()
        {
            var state = new CartState();
            while (true)
            {
                var allCartItems = await _cartService.GetCartItemsAsync(_sessionService.CurrentUser.UserID);

                state.TotalPages = (int)Math.Ceiling(allCartItems.Count / (double)state.PageSize);
                if (state.TotalPages == 0) state.TotalPages = 1;
                if (state.CurrentPage > state.TotalPages) state.CurrentPage = state.TotalPages;

                var pagedItems = allCartItems.Skip((state.CurrentPage - 1) * state.PageSize).Take(state.PageSize).ToList();

                var menuContent = new Markup(
                    "[bold yellow underline]TÙY CHỌN GIỎ HÀNG[/]\n" +
                    "1. Cập nhật sản phẩm\n" +
                    "2. Xóa sản phẩm\n" +
                    "[bold green]3. TIẾN HÀNH THANH TOÁN[/]\n\n" +
                    "[red]0. Quay về Menu chính[/]"
                );

                var totalAmount = await CalculateTotalAmountAsync(allCartItems);
                var viewContent = await CreateViewContentAsync(pagedItems, totalAmount);
                var notification = new Markup(
                    $"Trang [bold yellow]{state.CurrentPage}[/] / [bold yellow]{state.TotalPages}[/]. " +
                    "Lệnh: [blue]'n'[/](Sau), [blue]'p'[/](Trước), [blue]'p.số'[/](Đến trang)\n" +
                    "Hoặc: [blue]'id.{mã sp}'[/](Xem chi tiết), [red]'exit'[/](Thoát)"
                    );
                _layout.Render(menuContent, viewContent, notification);

                Console.Write("\n> Nhập lựa chọn: ");
                string choice = Console.ReadLine()?.Trim().ToLower() ?? "";

                if (await HandleCommand(choice, state, allCartItems) == false)
                {
                    return;
                }
            }
        }

        private async Task<bool> HandleCommand(string choice, CartState state, List<CartItem> allItems)
        {
            if (choice.StartsWith("id."))
            {
                if (int.TryParse(choice.AsSpan(3), out int productId))
                {
                    await _productMenu.HandleViewProductDetailsAsync(productId);
                }
                return true;
            }
            if (choice.StartsWith("p."))
            {
                if (int.TryParse(choice.AsSpan(2), out int page) && page > 0 && page <= state.TotalPages)
                {
                    state.CurrentPage = page;
                }
                return true;
            }

            switch (choice)
            {
                case "1": await HandleUpdateCartAsync(allItems); break;
                case "2": await HandleRemoveItemAsync(allItems); break;
                case "3":
                    // HIỆN THỰC THANH TOÁN GIỎ HÀNG
                    await HandleCheckoutCart(allItems);
                    break;
                case "n":
                    if (state.CurrentPage < state.TotalPages) state.CurrentPage++;
                    break;
                case "p":
                    if (state.CurrentPage > 1) state.CurrentPage--;
                    break;
                case "0":
                case "exit":
                    return false;
            }
            return true;
        }

        // PHƯƠNG THỨC MỚI: Xử lý thanh toán giỏ hàng
        private async Task HandleCheckoutCart(List<CartItem> allItems)
        {
            if (!allItems.Any())
            {
                AnsiConsole.MarkupLine("[red]Giỏ hàng trống, không thể thanh toán.[/]");
                Console.ReadKey();
                return;
            }

            // Gọi luồng thanh toán và nhận kết quả
            bool isSuccess = await _checkoutMenu.StartCheckoutFlowAsync(allItems);

            // Chỉ xóa giỏ hàng nếu giao dịch thành công
            if (isSuccess)
            {
                await _cartService.ClearCartAsync(_sessionService.CurrentUser.UserID);
                AnsiConsole.MarkupLine("\n[green]Đã dọn dẹp giỏ hàng sau khi hoàn tất thanh toán.[/]");
                Console.ReadKey();
            }
        }

        // ... (Các phương thức khác của CartMenu giữ nguyên)
        #region Other CartMenu Methods
        private async Task<IRenderable> CreateViewContentAsync(List<CartItem> pagedItems, decimal totalAmount)
        {
            var totalPanel = new Panel(new Markup($"[bold yellow]TỔNG GIÁ ĐƠN HÀNG: {totalAmount:N0} VNĐ[/]"))
                .Header(new PanelHeader("TỔNG KẾT").Centered())
                .Border(BoxBorder.Double)
                .Expand();

            var cartTable = await CreatePagedCartTableAsync(pagedItems);

            return new Rows(totalPanel, cartTable);
        }

        private async Task<Table> CreatePagedCartTableAsync(List<CartItem> cartItems)
        {
            var table = new Table().Expand().Border(TableBorder.HeavyHead);

            table.AddColumn(new TableColumn("[yellow]ID Giỏ hàng[/]").Centered());
            table.AddColumn(new TableColumn("[yellow]ID Sản phẩm[/]").Centered());
            table.AddColumn(new TableColumn("[yellow]Tên sản phẩm[/]"));
            table.AddColumn(new TableColumn("[yellow]Size[/]").Centered());
            table.AddColumn(new TableColumn("[yellow]Số lượng[/]").Centered());
            table.AddColumn(new TableColumn("[yellow]Đơn giá[/]") { Alignment = Justify.Right });
            table.AddColumn(new TableColumn("[yellow]Thành tiền[/]") { Alignment = Justify.Right });

            if (!cartItems.Any())
            {
                table.AddRow(new Markup(""), new Markup("[red]Giỏ hàng của bạn đang trống.[/]")).Centered();
                return table;
            }

            foreach (var item in cartItems)
            {
                var (discountedPrice, _) = await _promotionService.CalculateDiscountedPriceAsync(item.Product);
                var unitPrice = discountedPrice ?? item.Product.Price;
                var subTotal = unitPrice * item.Quantity;

                table.AddRow(
                    new Markup(item.CartItemID.ToString()),
                    new Markup(item.ProductID.ToString()),
                    new Markup(Markup.Escape(item.Product.Name)),
                    new Markup(item.Size.ToString()),
                    new Markup(item.Quantity.ToString()),
                    new Markup($"[green]{unitPrice:N0} VNĐ[/]"),
                    new Markup($"[bold green]{subTotal:N0} VNĐ[/]")
                );
            }
            return table;
        }

        private async Task<decimal> CalculateTotalAmountAsync(List<CartItem> allItems)
        {
            decimal totalAmount = 0;
            foreach (var item in allItems)
            {
                var (discountedPrice, _) = await _promotionService.CalculateDiscountedPriceAsync(item.Product);
                var unitPrice = discountedPrice ?? item.Product.Price;
                totalAmount += unitPrice * item.Quantity;
            }
            return totalAmount;
        }

        private async Task HandleUpdateCartAsync(List<CartItem> cartItems)
        {
            if (!cartItems.Any())
            {
                AnsiConsole.MarkupLine("[red]Giỏ hàng của bạn đang trống.[/]");
                Console.ReadKey();
                return;
            }

            string inputId = AnsiConsole.Ask<string>("Nhập [green]ID Giỏ hàng[/] của sản phẩm muốn cập nhật (hoặc '[red]exit[/]'):");
            if (inputId.Equals("exit", StringComparison.OrdinalIgnoreCase)) return;

            if (!int.TryParse(inputId, out int cartItemId) || cartItems.All(ci => ci.CartItemID != cartItemId))
            {
                AnsiConsole.MarkupLine("[red]ID không hợp lệ. Vui lòng thử lại.[/]");
                Console.ReadKey();
                return;
            }

            var itemToUpdate = cartItems.First(ci => ci.CartItemID == cartItemId);

            AnsiConsole.Clear();
            DisplayItemInfo(itemToUpdate);
            var choice = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("\n[bold]Chọn hành động bạn muốn:[/]")
                    .AddChoices(new[] { "Cập nhật số lượng", "Cập nhật size", "[red]Quay lại[/]" }));

            switch (choice)
            {
                case "Cập nhật số lượng": await HandleUpdateQuantityAsync(itemToUpdate); break;
                case "Cập nhật size": await HandleUpdateSize_HybridSolution(itemToUpdate); break;
                case "[red]Quay lại[/]": return;
            }
        }

        private async Task HandleUpdateQuantityAsync(CartItem itemToUpdate)
        {
            int stock = itemToUpdate.Product.ProductSizes.FirstOrDefault(ps => ps.Size == itemToUpdate.Size)?.QuantityInStock ?? 0;

            AnsiConsole.Clear();
            DisplayItemInfo(itemToUpdate);

            var newQuantity = AnsiConsole.Prompt(
                new TextPrompt<int>($"\nNhập [green]số lượng mới[/] (Tối đa: 5, Tồn kho: {stock}):")
                    .ValidationErrorMessage("[red]Dữ liệu không hợp lệ![/]")
                    .Validate(q =>
                    {
                        if (q <= 0) return ValidationResult.Error("[red]Số lượng phải lớn hơn 0.[/]");
                        if (q > 5) return ValidationResult.Error("[red]Chỉ được mua tối đa 5 sản phẩm.[/]");
                        if (q > stock) return ValidationResult.Error($"[red]Số lượng tồn kho không đủ (chỉ còn {stock}).[/]");
                        return ValidationResult.Success();
                    })
            );

            if (AnsiConsole.Confirm($"Bạn có chắc muốn cập nhật số lượng thành [yellow]{newQuantity}[/]?"))
            {
                var response = await _cartService.UpdateCartItemQuantityAsync(itemToUpdate.CartItemID, newQuantity);
                string color = response.IsSuccess ? "green" : "red";
                AnsiConsole.MarkupLine($"[{color}]{Markup.Escape(response.Message)}[/]");
                Console.ReadKey();
            }
            else
            {
                AnsiConsole.MarkupLine("[yellow]Đã hủy thao tác.[/]");
                Console.ReadKey();
            }
        }

        private async Task HandleUpdateSize_HybridSolution(CartItem itemToUpdate)
        {
            Console.Clear();
            DisplayItemInfo(itemToUpdate);

            var availableSizes = itemToUpdate.Product.ProductSizes
                .Where(ps => (ps.QuantityInStock ?? 0) >= itemToUpdate.Quantity && ps.Size != itemToUpdate.Size)
                .ToList();

            if (!availableSizes.Any())
            {
                AnsiConsole.MarkupLine("\n[red]Sản phẩm này không có size khác phù hợp hoặc đủ hàng.[/]");
                Console.ReadKey();
                return;
            }

            Console.WriteLine("\nCác size có sẵn để đổi:");
            for (int i = 0; i < availableSizes.Count; i++)
            {
                Console.WriteLine($"  {i + 1}. Size {availableSizes[i].Size} (Tồn kho: {availableSizes[i].QuantityInStock})");
            }

            int choiceIndex;
            ProductSize newSize;
            while (true)
            {
                Console.Write("\nNhập số thứ tự của size bạn muốn chọn (hoặc 'exit'): ");
                string input = Console.ReadLine();
                if (input.Equals("exit", StringComparison.OrdinalIgnoreCase)) return;

                if (int.TryParse(input, out choiceIndex) && choiceIndex > 0 && choiceIndex <= availableSizes.Count)
                {
                    newSize = availableSizes[choiceIndex - 1];
                    break;
                }
                Console.WriteLine("Lựa chọn không hợp lệ, vui lòng nhập lại.");
            }

            Console.WriteLine($"\n(Lưu ý: Nếu size {newSize.Size} đã có trong giỏ, chúng sẽ được gộp lại)");
            Console.Write($"Bạn có chắc muốn đổi từ size {itemToUpdate.Size} sang size {newSize.Size}? (y/n): ");
            string confirm = Console.ReadLine();

            if (confirm.Equals("y", StringComparison.OrdinalIgnoreCase))
            {
                var response = await _cartService.UpdateCartItemSizeAsync(_sessionService.CurrentUser.UserID, itemToUpdate.CartItemID, newSize.Size);

                string color = response.IsSuccess ? "green" : "red";
                AnsiConsole.MarkupLine($"\n[{color}]{Markup.Escape(response.Message)}[/]");
                Console.ReadKey();
            }
            else
            {
                AnsiConsole.MarkupLine("\n[yellow]Đã hủy thao tác.[/]");
                Console.ReadKey();
            }
        }

        private async Task HandleRemoveItemAsync(List<CartItem> cartItems)
        {
            if (!cartItems.Any())
            {
                AnsiConsole.MarkupLine("[red]Giỏ hàng của bạn đang trống.[/]");
                Console.ReadKey();
                return;
            }

            var input = AnsiConsole.Ask<string>("Nhập [green]ID sản phẩm[/] cần xóa (cách nhau bởi dấu phẩy ',')\nHoặc nhập '[yellow]del.all[/]' để xóa tất cả (hoặc '[red]exit[/]'):");

            if (input.Equals("exit", StringComparison.OrdinalIgnoreCase)) return;

            if (input.Equals("del.all", StringComparison.OrdinalIgnoreCase))
            {
                if (AnsiConsole.Confirm("[bold red]Bạn có chắc chắn muốn xóa TẤT CẢ sản phẩm khỏi giỏ hàng không?[/]"))
                {
                    await _cartService.ClearCartAsync(_sessionService.CurrentUser.UserID);
                    AnsiConsole.MarkupLine("[green]Đã xóa toàn bộ giỏ hàng.[/]");
                    Console.ReadKey();
                }
            }
            else
            {
                var idsToRemove = input.Split(',')
                    .Select(idStr => int.TryParse(idStr.Trim(), out int id) ? id : -1)
                    .Where(id => id != -1)
                    .ToList();

                if (!idsToRemove.Any())
                {
                    AnsiConsole.MarkupLine("[red]Không có ID hợp lệ nào được nhập.[/]");
                    Console.ReadKey();
                    return;
                }

                if (AnsiConsole.Confirm($"[bold red]Bạn có chắc chắn muốn xóa {idsToRemove.Count} sản phẩm khỏi giỏ hàng không?[/]"))
                {
                    await _cartService.RemoveCartItemsAsync(_sessionService.CurrentUser.UserID, idsToRemove);
                    AnsiConsole.MarkupLine("[green]Các sản phẩm đã chọn đã được xóa.[/]");
                    Console.ReadKey();
                }
            }
        }

        private void DisplayItemInfo(CartItem item)
        {
            AnsiConsole.Write(new Rule($"[yellow]Thông tin sản phẩm - ID: {item.CartItemID}[/]").Centered());

            AnsiConsole.Markup("[bold]Sản phẩm:[/] ");
            AnsiConsole.Write(Markup.Escape(item.Product.Name));
            AnsiConsole.WriteLine();

            AnsiConsole.MarkupLine($"[bold]Size hiện tại:[/] {item.Size}");
            AnsiConsole.MarkupLine($"[bold]Số lượng:[/] {item.Quantity}");

            AnsiConsole.Write(new Rule());
        }
        #endregion
    }
}