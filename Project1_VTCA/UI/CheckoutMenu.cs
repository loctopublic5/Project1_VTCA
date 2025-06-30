using Project1_VTCA.Data;
using Project1_VTCA.Services.Interface;
using Project1_VTCA.UI.Interface;
using Spectre.Console;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Project1_VTCA.UI
{
    public class CheckoutMenu : ICheckoutMenu
    {
        private readonly IOrderService _orderService;
        private readonly IPromotionService _promotionService;
        private readonly ISessionService _sessionService;
        private readonly IAddressService _addressService;

        public CheckoutMenu(IOrderService orderService, IPromotionService promotionService, ISessionService sessionService, IAddressService addressService)
        {
            _orderService = orderService;
            _promotionService = promotionService;
            _sessionService = sessionService;
            _addressService = addressService;
        }

        // CẬP NHẬT: Trả về Task<bool>
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

            // TÍCH HỢP: Chọn địa chỉ từ danh sách thật
            var selectedAddress = await ChooseShippingAddress();
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

            // CHUẨN HÓA: Lời gọi phương thức giờ đây truyền đúng số lượng và đúng loại tham số.
            var response = await _orderService.CreateOrderAsync(
            _sessionService.CurrentUser.UserID,
            itemsToCheckout,
            $"{selectedAddress.AddressDetail}, {selectedAddress.City}",
            selectedAddress.ReceivePhone,
            paymentMethod
        );

            if (response.IsSuccess)
            {
                // CẬP NHẬT: Trừ tiền vào số dư session ngay lập tức nếu thành công
                if (paymentMethod == "Thanh toán ngay (trừ vào số dư)")
                {
                    _sessionService.CurrentUser.Balance -= totalPrice;
                }

                DisplaySuccessReceipt(response.Message, totalPrice);
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
        

        // PHƯƠNG THỨC MỚI: Tích hợp AddressService
        private async Task<Address> ChooseShippingAddress()
        {
            var addresses = await _addressService.GetActiveAddressesAsync(_sessionService.CurrentUser.UserID);
            if (!addresses.Any())
            {
                AnsiConsole.MarkupLine("[red]Bạn chưa có địa chỉ nào. Vui lòng thêm địa chỉ trong mục 'Quản lý tài khoản' trước.[/]");
                return null;
            }

            var prompt = new SelectionPrompt<Address>()
                .Title("\nChọn [green]địa chỉ nhận hàng[/]:")
                .PageSize(7)
                .UseConverter(addr => {
                    var displayText = $"{addr.AddressDetail}, {addr.City} - SĐT: {addr.ReceivePhone}";
                    return addr.IsDefault ? $"[bold yellow](Mặc định)[/] {Markup.Escape(displayText)}" : Markup.Escape(displayText);
                })
                .AddChoices(addresses);

            return AnsiConsole.Prompt(prompt);
        }

        // ... các phương thức khác giữ nguyên
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

                if (paymentMethod.Equals("exit", System.StringComparison.OrdinalIgnoreCase)) return null;

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
                    $"{(unitPrice * item.Quantity):N0} VNĐ"
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

        private void DisplaySuccessReceipt(string orderCode, decimal total)
        {
            AnsiConsole.Clear();
            var panel = new Panel(
                new Rows(
                    new Markup("[green bold]Cảm ơn bạn đã mua hàng![/]"),
                    new Markup($"Mã đơn hàng của bạn là: [yellow bold]{orderCode}[/]"),
                    new Markup($"Tổng số tiền: [yellow bold]{total:N0} VNĐ[/]"),
                    new Text(""),
                    new Markup("[dim]Đơn hàng của bạn đang chờ quản trị viên xác nhận.[/]"),
                    new Markup("[dim]Bạn có thể theo dõi trạng thái trong mục 'Quản lý tài khoản'.[/]")
                ))
                .Header(new PanelHeader("ĐẶT HÀNG THÀNH CÔNG").Centered())
                .Border(BoxBorder.Double)
                .Padding(2, 1)
                .Expand();
            AnsiConsole.Write(panel);
        }
        #endregion
    }
}