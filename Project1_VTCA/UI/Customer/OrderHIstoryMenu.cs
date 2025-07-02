using Project1_VTCA.Data;
using Project1_VTCA.Services.Interface;
using Project1_VTCA.UI.Customer.Interfaces;
using Project1_VTCA.UI.Draw;
using Spectre.Console;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Project1_VTCA.UI.Customer
{
    public class OrderHistoryMenu : IOrderHistoryMenu
    {
        private readonly IOrderService _orderService;
        private readonly ISessionService _sessionService;
        private readonly ConsoleLayout _layout;

        public OrderHistoryMenu(IOrderService orderService, ISessionService sessionService, ConsoleLayout layout)
        {
            _orderService = orderService;
            _sessionService = sessionService;
            _layout = layout;
        }

        public async Task ShowAsync()
        {
            string? currentFilter = null;
            int pageNumber = 1; // Default page number
            int pageSize = 10;  // Default page size

            while (true)
            {
                var (orders, totalPages) = await _orderService.GetOrdersAsync(
                    _sessionService.CurrentUser.UserID, currentFilter, pageNumber, pageSize);

                var menuContent = CreateMenu();
                var viewContent = CreateOrderTable(orders);
                var notification = new Markup("[dim]Chọn bộ lọc, hoặc nhập 'or.{id}' để xem chi tiết.[/]");

                _layout.Render(menuContent, viewContent, notification);

                var choice = AnsiConsole.Ask<string>("\n> Nhập lựa chọn:").ToLower();

                if (choice.StartsWith("or."))
                {
                    if (int.TryParse(choice.AsSpan(3), out int orderId))
                    {
                        await HandleViewOrderDetailsAsync(orderId);
                    }
                    continue;
                }

                switch (choice)
                {
                    case "1": currentFilter = null; break;
                    case "2": currentFilter = "PendingAdminApproval"; break;
                    case "3": currentFilter = "Processing"; break;
                    case "4": currentFilter = "CancellationRequested"; break;
                    case "5":
                        currentFilter = "RejectedByAdmin|Cancelled|CustomerCancelled";
                        break;
                    case "6": await HandleCancelOrder(); break;
                    case "n": if (pageNumber < totalPages) pageNumber++; break; // Next page
                    case "p": if (pageNumber > 1) pageNumber--; break; // Previous page
                    case "0": return;
                    default: break;
                }
            }
        }


        // ... (các phương thức khác của OrderHistoryMenu)

        #region Helper Methods
        private Markup CreateMenu()
        {
            return new Markup(
               "[bold yellow underline]LỊCH SỬ ĐƠN HÀNG[/]\n\n" +
"[bold]Lọc theo trạng thái:[/]\n" +
" 1. Xem Tất cả\n" +
" 2. Đơn hàng Chờ xác nhận\n" +
" 3. Đơn hàng Đã xác nhận\n" +
" 4. Đơn hàng chờ huỷ\n" +
" 5. Đơn hàng Đã hủy\n\n" +
"[bold]Hành động:[/]\n" +
" 6. Hủy một đơn hàng\n\n" +
" [red]0. Quay lại[/]"
            );
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

        private Table CreateOrderTable(List<Order> orders)
        {
            var table = new Table().Expand().Border(TableBorder.Rounded);
            table.Title("DANH SÁCH ĐƠN HÀNG");
            table.AddColumn(new TableColumn("ID Đơn hàng"));
            table.AddColumn(new TableColumn("Mã Đơn"));
            table.AddColumn(new TableColumn("Ngày đặt"));
            table.AddColumn(new TableColumn("Tổng tiền"));
            table.AddColumn(new TableColumn("Trạng thái"));

            if (!orders.Any())
            {
                table.AddRow(new Markup(""), new Markup("[grey]Không có đơn hàng nào.[/]"), new Markup(""), new Markup(""), new Markup(""));
                return table;
            }

            foreach (var order in orders)
            {
                table.AddRow(
                    new Markup(Markup.Escape(order.OrderID.ToString())),
                    new Markup(Markup.Escape(order.OrderCode)),
                    new Markup(Markup.Escape(order.OrderDate.ToString("dd/MM/yyyy HH:mm"))),
                    new Markup($"[yellow]{order.TotalPrice:N0} VNĐ[/]"),
                    FormatOrderStatus(order.Status)
                );
            }
            return table;
        }


        private async Task HandleViewOrderDetailsAsync(int orderId)
        {
            var order = await _orderService.GetOrderByIdAsync(orderId, _sessionService.CurrentUser.UserID);
            if (order == null)
            {
                AnsiConsole.MarkupLine("[red]Không tìm thấy đơn hàng với ID này.[/]");
                Console.ReadKey();
                return;
            }

            
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
            table.AddColumn("Sản phẩm");
            table.AddColumn("Size");
            table.AddColumn("Số lượng");
            table.AddColumn("Đơn giá");
            table.AddColumn("Thành tiền");

            foreach (var detail in order.OrderDetails)
            {
                table.AddRow(
                    new Markup(Markup.Escape(detail.Product.Name)),
                    new Markup(Markup.Escape(detail.Size.ToString())),
                    new Markup(Markup.Escape(detail.Quantity.ToString())),
                    new Markup($"{detail.UnitPrice:N0} VNĐ"),
                    new Markup($"[bold]{detail.UnitPrice * detail.Quantity:N0} VNĐ[/]")
                );
            }

            AnsiConsole.Clear();
            AnsiConsole.Write(panel);
            AnsiConsole.Write(table);
            AnsiConsole.MarkupLine($"\n[bold yellow]TỔNG TIỀN THANH TOÁN: {order.TotalPrice:N0} VNĐ[/]");
            AnsiConsole.MarkupLine("\n[dim]Nhấn phím bất kỳ để quay lại...[/]");
            Console.ReadKey();
        }

        // NÂNG CẤP: Yêu cầu nhập OrderID (int) thay vì OrderCode (string)
        private async Task HandleCancelOrder()
        {
            var orderId = AnsiConsole.Ask<int>("Nhập [green]ID Đơn hàng[/] bạn muốn yêu cầu hủy (chỉ nhập số):");

            var reason = AnsiConsole.Ask<string>("Nhập [green]lý do hủy[/]:");
            if (string.IsNullOrWhiteSpace(reason))
            {
                AnsiConsole.MarkupLine("[red]Lý do hủy không được để trống.[/]");
                Console.ReadKey();
                return;
            }

            if (AnsiConsole.Confirm($"Bạn có chắc muốn yêu cầu hủy đơn hàng [yellow]ID {orderId}[/]?"))
            {
                var response = await _orderService.RequestCancellationAsync(_sessionService.CurrentUser.UserID, orderId, reason);
                string color = response.IsSuccess ? "green" : "red";
                AnsiConsole.MarkupLine($"\n[{color}]{Markup.Escape(response.Message)}[/]");
                Console.ReadKey();
            }
        }

        #endregion
    }
}