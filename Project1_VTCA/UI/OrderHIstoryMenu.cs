using Project1_VTCA.Data;
using Project1_VTCA.Services.Interface;
using Project1_VTCA.UI.Draw;
using Project1_VTCA.UI.Interface;
using Spectre.Console;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Project1_VTCA.UI
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
            while (true)
            {
                var orders = await _orderService.GetOrdersAsync(_sessionService.CurrentUser.UserID, currentFilter);

                var menuContent = CreateMenu();
                var viewContent = CreateOrderTable(orders);
                var notification = new Markup("[dim]Chọn một bộ lọc từ menu hoặc một hành động.[/]");

                _layout.Render(menuContent, viewContent, notification);

                var choice = AnsiConsole.Ask<string>("\n> Nhập lựa chọn:").ToLower();

                switch (choice)
                {
                    case "1": currentFilter = null; break;
                    case "2": currentFilter = "PendingAdminApproval"; break;
                    case "3": currentFilter = "Processing"; break;
                    case "4": currentFilter = "Cancelled"; break;
                    case "5": await HandleCancelOrder(); break;
                    case "0": return;
                    default: break;
                }
            }
        }

        private Markup CreateMenu()
        {
            return new Markup(
                "[bold yellow underline]LỊCH SỬ ĐƠN HÀNG[/]\n\n" +
                "[bold]Lọc theo trạng thái:[/]\n" +
                " [1] Xem Tất cả\n" +
                " [2] Đơn hàng Chờ xác nhận\n" +
                " [3] Đơn hàng Đã xác nhận\n" +
                " [4] Đơn hàng Đã hủy\n\n" +
                "[bold]Hành động:[/]\n" +
                " [5] Hủy một đơn hàng\n\n" +
                " [red][0] Quay lại[/]"
            );
        }

        private Table CreateOrderTable(List<Order> orders)
        {
            var table = new Table().Expand().Border(TableBorder.Rounded);
            table.Title("DANH SÁCH ĐƠN HÀNG");
            table.AddColumn("ID Đơn hàng");
            table.AddColumn("Mã Đơn");
            table.AddColumn("Ngày đặt");
            table.AddColumn("Tổng tiền");
            table.AddColumn("Trạng thái");

            if (!orders.Any())
            {
                table.AddRow("", "[grey]Không có đơn hàng nào.[/]", "", "", "");
                return table;
            }

            foreach (var order in orders)
            {
                table.AddRow(
              new Markup(Markup.Escape(order.OrderID.ToString())),
              new Markup(Markup.Escape(order.OrderCode)),
              new Markup(Markup.Escape(order.OrderDate.ToString("dd/MM/yyyy HH:mm"))),
              new Markup($"[yellow]{Markup.Escape(order.TotalPrice.ToString("N0"))} VNĐ[/]"),
              new Markup(Markup.Escape(order.Status ?? "N/A"))
          );
            }
            return table;
        }

        private async Task HandleCancelOrder()
        {
            var orderIdStr = AnsiConsole.Ask<string>("Nhập [green]ID Đơn hàng[/] bạn muốn yêu cầu hủy (chỉ nhập số):");

            if (!int.TryParse(orderIdStr, out int orderId))
            {
                AnsiConsole.MarkupLine("[red]ID đơn hàng không hợp lệ.[/]");
                Console.ReadKey();
                return;
            }

            var reason = AnsiConsole.Ask<string>("Nhập [green]lý do hủy[/]:");

            if (string.IsNullOrWhiteSpace(reason))
            {
                AnsiConsole.MarkupLine("[red]Lý do hủy không được để trống.[/]");
                Console.ReadKey();
                return;
            }

            if (!AnsiConsole.Confirm($"Bạn có chắc muốn yêu cầu hủy đơn hàng [yellow]ID {orderId}[/]?"))
            {
                AnsiConsole.MarkupLine("[yellow]Đã hủy thao tác.[/]");
                Console.ReadKey();
                return;
            }

            var response = await _orderService.RequestCancellationAsync(_sessionService.CurrentUser.UserID, orderId, reason);
            string color = response.IsSuccess ? "green" : "red";
            AnsiConsole.MarkupLine($"\n[{color}]{Markup.Escape(response.Message)}[/]");
            Console.ReadKey();
        }
    }
}