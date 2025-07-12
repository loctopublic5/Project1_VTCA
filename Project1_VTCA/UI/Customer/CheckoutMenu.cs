using Microsoft.Extensions.DependencyInjection;
using Project1_VTCA.Data;
using Project1_VTCA.Services.Interface;
using Project1_VTCA.UI.Customer.Interfaces;
using Spectre.Console;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Project1_VTCA.UI.Customer
{
    public class CheckoutMenu : ICheckoutMenu
    {
        private readonly IOrderService _orderService;
        private readonly IPromotionService _promotionService;
        private readonly ISessionService _sessionService;
        private readonly IAddressService _addressService;
        private readonly IServiceProvider _serviceProvider;

        public CheckoutMenu(IOrderService orderService, IPromotionService promotionService, ISessionService sessionService, IAddressService addressService, IServiceProvider serviceProvider)
        {
            _orderService = orderService;
            _promotionService = promotionService;
            _sessionService = sessionService;
            _addressService = addressService;
            _serviceProvider = serviceProvider;
        }

     
        public async Task<bool> StartCheckoutFlowAsync(List<CartItem> itemsToCheckout)
        {
            if (!itemsToCheckout.Any())
            {
                AnsiConsole.MarkupLine("[red]Không có sản phẩm nào để thanh toán.[/]");
                Console.ReadKey();
                return false;
            }

            AnsiConsole.Clear();

            var (subTotal, totalQuantity) = await CalculateTotals(itemsToCheckout);
            var shippingFee = _orderService.CalculateShippingFee(totalQuantity);
            var totalPrice = subTotal + shippingFee;

            AnsiConsole.Write(await CreateSummaryTable(itemsToCheckout, subTotal, shippingFee, totalPrice));

            var selectedAddress = await ChooseOrAddAddressAsync();
            if (selectedAddress == null) { AnsiConsole.MarkupLine("[yellow]Đã hủy thanh toán.[/]"); Console.ReadKey(); return false; }

            

            string paymentMethod = await ChoosePaymentMethod(totalPrice);
            if (paymentMethod == null) { AnsiConsole.MarkupLine("[yellow]Đã hủy thanh toán.[/]"); Console.ReadKey(); return false; }


            var finalConfirmPanel = CreateFinalConfirmPanel(selectedAddress, paymentMethod, totalPrice);
            AnsiConsole.Write(finalConfirmPanel);


            if (!AnsiConsole.Confirm("\n[bold yellow]Xác nhận đặt hàng với các thông tin trên?[/]"))
            {
                AnsiConsole.MarkupLine("[yellow]Đã hủy thao tác đặt hàng.[/]");
                Console.ReadKey();
                return false;
            }


            var response = await _orderService.CreateOrderAsync(
                _sessionService.CurrentUser.UserID,
                itemsToCheckout, $"{selectedAddress.AddressDetail}," +
                $" {selectedAddress.City}",
                selectedAddress.ReceivePhone,
                paymentMethod);

            if (response.IsSuccess && int.TryParse(response.Message, out int newOrderId))
            {
                var newOrder = await _orderService.GetOrderByIdAsync(newOrderId, _sessionService.CurrentUser.UserID);
                if (newOrder != null)
                {
            
                    if (newOrder.PaymentMethod == "Thanh toán ngay (trừ vào số dư)")
                    {
                        _sessionService.CurrentUser.Balance -= newOrder.TotalPrice;
                    }
                    DisplaySuccessReceipt(newOrder);
                }
                Console.ReadKey();
                return true;
            }
            else
            {
                AnsiConsole.MarkupLine($"[red]Lỗi khi tạo đơn hàng: {Markup.Escape(response.Message)}[/]");
                Console.ReadKey();
                return false;
            }
        }


        private async Task<Address?> ChooseOrAddAddressAsync()
        {
            var addressService = _serviceProvider.GetRequiredService<IAddressService>();
            while (true)
            {
                var addresses = await addressService.GetActiveAddressesAsync(_sessionService.CurrentUser.UserID);
                if (!addresses.Any())
                {
                    if (AnsiConsole.Confirm("[yellow]Bạn chưa có địa chỉ nào. Bạn có muốn tạo một địa chỉ nhận hàng ngay bây giờ không?[/]"))
                    {
                        var addressMenu = _serviceProvider.GetRequiredService<IAddressMenu>();
                      
                        var newAddress = await addressMenu.HandleAddAddressFlowAsync(true);
                        if (newAddress != null)
                        {
                            return newAddress; 
                        }
                    }
                    else
                    {
                        return null; 
                    }
                }
                else
                {
      
                    var prompt = new SelectionPrompt<Address>()
                        .Title("\nChọn [green]địa chỉ nhận hàng[/]:")
                        .UseConverter(addr => {
                            var displayText = $"{addr.AddressDetail}, {addr.City} - SĐT: {addr.ReceivePhone}";
                            return addr.IsDefault ? $"[bold yellow](Mặc định)[/] {Markup.Escape(displayText)}" : Markup.Escape(displayText);
                        })
                        .AddChoices(addresses);
                    return AnsiConsole.Prompt(prompt);
                }
            }
        }

    
        #region Other CheckoutMenu Methods
        private async Task<(decimal, int)> CalculateTotals(List<CartItem> items)
        {
            decimal subTotal = 0;
            int totalQuantity = 0;
            foreach (var item in items)
            {
                var (discountedPrice, _) = await _promotionService.CalculateDiscountedPriceAsync(item.Product);
                subTotal += (discountedPrice ?? item.Product.Price) * item.Quantity;
                totalQuantity += item.Quantity;
            }
            return (subTotal, totalQuantity);
        }

        private async Task<string> ChoosePaymentMethod(decimal totalPrice)
        {
            while (true)
            {
                var paymentMethod = AnsiConsole.Prompt(
                    new SelectionPrompt<string>()
                        .Title("\nChọn [green]phương thức thanh toán[/] (nhập '[red]exit[/]' để hủy):")
                        .AddChoices(new[] { "Thanh toán khi nhận hàng (COD)", "Thanh toán ngay (trừ vào số dư)", "exit" })
                );

                if (paymentMethod.Equals("exit", StringComparison.OrdinalIgnoreCase)) return null;

                if (paymentMethod == "Thanh toán ngay (trừ vào số dư)")
                {
                    if (_sessionService.CurrentUser.Balance < totalPrice)
                    {
                        AnsiConsole.MarkupLine($"[red]Số dư không đủ! Cần {totalPrice:N0} VNĐ, bạn chỉ có {_sessionService.CurrentUser.Balance:N0} VNĐ.[/]");

                        var choice = AnsiConsole.Prompt(
                            new SelectionPrompt<string>()
                            .Title("Bạn muốn làm gì tiếp theo?")
                            .AddChoices(new[] { "Thử lại với phương thức khác", "Hủy bỏ thanh toán" })
                        );
                        if (choice == "Hủy bỏ thanh toán") return null;
                    }
                    else
                    {
                        return paymentMethod;
                    }
                }
                else
                {
                    return paymentMethod;
                }
            }
        }

        private async Task<Table> CreateSummaryTable(List<CartItem> items, decimal subTotal, decimal shippingFee, decimal totalPrice)
        {
            var table = new Table().Expand().Border(TableBorder.Rounded);
            table.Title = new TableTitle("[bold yellow]XÁC NHẬN ĐƠN HÀNG[/]");
            table.AddColumn("Sản phẩm");
            table.AddColumn("Size", c => c.Centered());
            table.AddColumn("SL", c => c.Centered());
            table.AddColumn("Thành tiền", c => c.RightAligned());

            foreach (var item in items)
            {
                var (discountedPrice, _) = await _promotionService.CalculateDiscountedPriceAsync(item.Product);
                var unitPrice = discountedPrice ?? item.Product.Price;
                table.AddRow(
                    Markup.Escape(item.Product.Name),
                    item.Size.ToString(),
                    item.Quantity.ToString(),
                    $"{unitPrice * item.Quantity:N0} VNĐ"
                );
            }

            table.AddEmptyRow();
            table.AddRow("", "", "[bold]Tổng tiền hàng[/]", $"[bold]{subTotal:N0} VNĐ[/]");
            table.AddRow("", "", "[bold]Phí vận chuyển[/]", $"[bold]{shippingFee:N0} VNĐ[/]");
            table.AddRow("", "", "[bold yellow]TỔNG TIỀN CUỐI CÙNG[/]", $"[bold yellow]{totalPrice:N0} VNĐ[/]");

            return table;
        }

        private Panel CreateFinalConfirmPanel(Address address, string payment, decimal total)
        {
            var fullAddress = $"{address.AddressDetail}, {address.City}";
            return new Panel(
                new Rows(
                    new Markup($"[bold]Địa chỉ nhận hàng:[/] {Markup.Escape(fullAddress)}"),
                    new Markup($"[bold]SĐT người nhận:[/] {Markup.Escape(address.ReceivePhone)}"),
                    new Markup($"[bold]Phương thức thanh toán:[/] {payment}"),
                    new Markup($"[bold]Tổng thanh toán:[/] [yellow]{total:N0} VNĐ[/]")
                ))
                .Header("TÓM TẮT CUỐI CÙNG")
                .Border(BoxBorder.Double);
        }

        private async Task DisplaySuccessReceipt(Order order)
        {
            if (order == null)
            {
                AnsiConsole.MarkupLine("[red]Lỗi: Không thể hiển thị chi tiết đơn hàng.[/]");
                Console.ReadKey();
                return;
            }

            AnsiConsole.Clear();

 
            AnsiConsole.Write(
                new FigletText("COMPLETE CHECKOUT")
                    .Centered()
                    .Color(Color.Green));

            var grid = new Grid();
            grid.AddColumn();
            grid.AddColumn();
            grid.AddRow(new Markup("[bold]Mã đơn hàng:[/]"), new Markup(Markup.Escape(order.OrderCode)));
            grid.AddRow(new Markup("[bold]Ngày đặt:[/]"), new Markup(Markup.Escape(order.OrderDate.ToString("g"))));
            grid.AddRow(new Markup("[bold]Trạng thái:[/]"), FormatOrderStatus(order.Status));
            grid.AddRow(new Markup("[bold]Địa chỉ nhận:[/]"), new Markup(Markup.Escape(order.ShippingAddress)));
            grid.AddRow(new Markup("[bold]SĐT Nhận:[/]"), new Markup(Markup.Escape(order.ShippingPhone)));
            grid.AddRow(new Markup("[bold]Thanh toán:[/]"), new Markup(Markup.Escape(order.PaymentMethod ?? "N/A")));

            var panel = new Panel(grid)
                .Header($"CHI TIẾT ĐƠN HÀNG - ID: {order.OrderID}")
                .Expand();

            var table = new Table().Expand().Border(TableBorder.Rounded);
            table.Title = new TableTitle("Sản phẩm trong đơn");
            table.AddColumn("Sản phẩm");
            table.AddColumn("Size");
            table.AddColumn("Số lượng");
            table.AddColumn("Đơn giá");
            table.AddColumn("Thành tiền");


            if (order.OrderDetails != null)
            {
                foreach (var detail in order.OrderDetails)
                {
                    table.AddRow(
                        new Markup(Markup.Escape(detail.Product.Name)),
                        new Markup(Markup.Escape(detail.Size.ToString())),
                        new Markup(Markup.Escape(detail.Quantity.ToString())),
                        new Markup($"{detail.UnitPrice:N0} VNĐ"),
                        new Markup($"[bold]{(detail.UnitPrice * detail.Quantity):N0} VNĐ[/]")
                    );
                }
            }

            AnsiConsole.Write(panel);
            AnsiConsole.Write(table);
            AnsiConsole.MarkupLine($"\n[bold yellow]TỔNG TIỀN THANH TOÁN: {order.TotalPrice:N0} VNĐ[/]");
            AnsiConsole.MarkupLine("\n[dim]Cảm ơn bạn đã mua hàng! Nhấn phím bất kỳ để quay lại...[/]");
            Console.ReadKey();
        }




        private Markup FormatOrderStatus(string status)
        {
            return status switch
            {
                "PendingAdminApproval" => new Markup("[yellow]WAIT[/]"),
                "Processing" => new Markup("[green]DONE[/]"),
                "Completed" => new Markup("[green]DONE[/]"),
                "RejectedByAdmin" => new Markup("[red]REJECTED[/]"),
                "CustomerCancelled" => new Markup("[red]CANCELLED[/]"),
                "CancellationRequested" => new Markup("[orange1]REQ_CANCEL[/]"),
                "Cancelled" => new Markup("[red]CANCELLED[/]"),
                _ => new Markup(Markup.Escape(status))
            };
        }
        #endregion
    }
}