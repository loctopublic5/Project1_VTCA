using Project1_VTCA.Data;
using Project1_VTCA.Services.Interface;
using Project1_VTCA.UI.Draw;
using Spectre.Console;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Project1_VTCA.UI
{
    public class CartMenu : ICartMenu
    {
        private readonly ICartService _cartService;
        private readonly ISessionService _sessionService;
        private readonly IPromotionService _promotionService;
        private readonly ConsoleLayout _layout;
        

        public CartMenu(ICartService cartService, ISessionService sessionService, IPromotionService promotionService, ConsoleLayout layout)
        {
            _cartService = cartService;
            _sessionService = sessionService;
            _promotionService = promotionService;
            _layout = layout;
            
        }

        public async Task ShowAsync()
        {
            while (true)
            {
                var cartItems = await _cartService.GetCartItemsAsync(_sessionService.CurrentUser.UserID);

                var menuContent = new Markup(
                    "[bold yellow underline]GIỎ HÀNG[/]\n" +
                    "1. Cập nhật số lượng\n" +
                    "2. Xóa sản phẩm\n" +
                    "3. Tiến hành Thanh toán\n\n" +
                    "[red]4. Quay về Menu chính[/]"
                );

                var cartTable = await CreateCartTableAsync(cartItems);
                var notification = new Markup("[dim]Chọn một hành động từ menu bên trái.[/]");

                _layout.Render(menuContent, cartTable, notification);

                Console.Write("\n> Nhập lựa chọn: ");
                string choice = Console.ReadLine()?.Trim() ?? "";

                switch (choice)
                {
                    case "1":
                        await HandleUpdateQuantity();
                        break;
                    case "2":
                        await HandleRemoveItem();
                        break;
                    case "3":
                        
                        break;
                    case "4":
                        return; // Thoát về UserMenu
                }
            }
        }

        private async Task HandleUpdateQuantity()
        {
            var cartItemId = AnsiConsole.Ask<int>("Nhập [green]ID Giỏ hàng[/] của sản phẩm muốn cập nhật:");
            var newQuantity = AnsiConsole.Ask<int>("Nhập [green]số lượng mới[/] (nhập 0 để xóa):");

            await _cartService.UpdateCartItemQuantityAsync(cartItemId, newQuantity);
            AnsiConsole.MarkupLine("\n[green]Số lượng sản phẩm đã được cập nhật thành công![/]");
            Console.ReadKey();
        }


        private async Task HandleRemoveItem()
        {
            var cartItemId = AnsiConsole.Ask<int>("Nhập [green]ID Giỏ hàng[/] của sản phẩm muốn xóa:");
            await _cartService.RemoveCartItemAsync(cartItemId);
            AnsiConsole.MarkupLine("\n[green]Đã xóa sản phẩm khỏi giỏ hàng![/]");
            Console.ReadKey();
        }


        private async Task<Table> CreateCartTableAsync(List<CartItem> cartItems)
        {
            var table = new Table().Expand().Border(TableBorder.HeavyHead);
            table.Title = new TableTitle("CÁC SẢN PHẨM TRONG GIỎ");

            table.AddColumn(new TableColumn("[yellow]ID[/]").Centered());
            table.AddColumn(new TableColumn("[yellow]Tên sản phẩm[/]"));
            table.AddColumn(new TableColumn("[yellow]Size[/]").Centered());
            table.AddColumn(new TableColumn("[yellow]SL[/]").Centered());
            table.AddColumn(new TableColumn("[yellow]Đơn giá[/]") { Alignment = Justify.Right });
            table.AddColumn(new TableColumn("[yellow]Thành tiền[/]") { Alignment = Justify.Right });


            if (!cartItems.Any())
            {
                table.AddRow(new Text("Giỏ hàng của bạn đang trống.", new Style(Color.Red))).Centered();
                return table;
            }

            decimal totalAmount = 0;

            foreach (var item in cartItems)
            {
                var (discountedPrice, _) = await _promotionService.CalculateDiscountedPriceAsync(item.Product);
                var unitPrice = discountedPrice ?? item.Product.Price;
                var subTotal = unitPrice * item.Quantity;
                totalAmount += subTotal;

                // --- SỬA LỖI Ở ĐÂY: Bọc tất cả các giá trị trong new Markup() ---
                table.AddRow(
                    new Markup(item.CartItemID.ToString()),
                    new Markup(Markup.Escape(item.Product.Name)),
                    new Markup(item.Size.ToString()),
                    new Markup(item.Quantity.ToString()),
                    new Markup($"[green]{unitPrice:N0}[/]"),
                    new Markup($"[bold green]{subTotal:N0} VNĐ[/]")
                );
            }

            table.AddRow(
                new Markup(""),
                new Markup(""),
                new Markup(""),
                new Markup(""),
                new Markup("[bold yellow]Tổng tiền tạm tính:[/]"),
                new Markup($"[bold yellow]{totalAmount:N0} VNĐ[/]")
            );

            return table;
        }
    }
}