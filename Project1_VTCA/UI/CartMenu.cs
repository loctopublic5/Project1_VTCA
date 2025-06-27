using Project1_VTCA.Data;
using Project1_VTCA.Services.Interface;
using Project1_VTCA.UI.Draw;
using Project1_VTCA.UI.Interface;
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
                    "1. Cập nhật số lượng (sắp có)\n" +
                    "2. Xóa sản phẩm (sắp có)\n" +
                    "3. Tiến hành Thanh toán (sắp có)\n\n" +
                    "[red]4. Quay về Menu chính[/]"
                );

                var cartTable = await CreateCartTableAsync(cartItems);
                var notification = new Markup("[dim]Chọn một hành động từ menu bên trái hoặc nhập '4' để thoát.[/]");

                _layout.Render(menuContent, cartTable, notification);

                Console.Write("\n> Nhập lựa chọn: ");
                string choice = Console.ReadLine()?.Trim() ?? "";

                switch (choice)
                {
                    case "1":
                        AnsiConsole.MarkupLine("[yellow]Chức năng 'Cập nhật số lượng' sẽ được hiện thực sau.[/]");
                        Console.ReadKey();
                        break;
                    case "2":
                        AnsiConsole.MarkupLine("[yellow]Chức năng 'Xóa sản phẩm' sẽ được hiện thực sau.[/]");
                        Console.ReadKey();
                        break;
                    case "3":
                        AnsiConsole.MarkupLine("[yellow]Chức năng 'Tiến hành Thanh toán' sẽ được hiện thực sau.[/]");
                        Console.ReadKey();
                        break;
                    case "4":
                        return;
                }
            }
        }

        private async Task<Table> CreateCartTableAsync(List<CartItem> cartItems)
        {
            var table = new Table().Expand().Border(TableBorder.HeavyHead);
            table.Title = new TableTitle("CÁC SẢN PHẨM TRONG GIỎ");

            table.AddColumn(new TableColumn("[yellow]ID Giỏ hàng[/]").Centered());
            table.AddColumn(new TableColumn("[yellow]Tên sản phẩm[/]"));
            table.AddColumn(new TableColumn("[yellow]Size[/]").Centered());
            table.AddColumn(new TableColumn("[yellow]Số lượng[/]").Centered());
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

                table.AddRow(
                    new Markup(item.CartItemID.ToString()),
                    new Markup(Markup.Escape(item.Product.Name)),
                    new Markup(item.Size.ToString()),
                    new Markup(item.Quantity.ToString()),
                    new Markup($"[green]{unitPrice:N0}[/]"),
                    new Markup($"[bold green]{subTotal:N0} VNĐ[/]")
                );
            }

            table.AddEmptyRow();
            table.AddRow(
                new Markup(""),
                new Markup(""),
                new Markup(""),
                new Markup(""),
                new Markup("[bold yellow]Tổng giá đơn hàng:[/]"),
                new Markup($"[bold yellow]{totalAmount:N0} VNĐ[/]")
            );

            return table;
        }
    }
}