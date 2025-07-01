using Project1_VTCA.Data;
using Project1_VTCA.Services.Interface;
using Project1_VTCA.UI.Admin.Interface;
using Project1_VTCA.UI.Draw;
using Spectre.Console;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Project1_VTCA.UI.Admin
{
    public class AdminOrderMenu : IAdminOrderMenu
    {
        private readonly IOrderService _orderService;
        private readonly ISessionService _sessionService;
        private readonly ConsoleLayout _layout;

        public AdminOrderMenu(IOrderService orderService, ISessionService sessionService, ConsoleLayout layout)
        {
            _orderService = orderService;
            _sessionService = sessionService;
            _layout = layout;
        }

        public async Task ShowAsync()
        {
            string? currentFilter = "ActionRequired"; // Mặc định hiển thị đơn cần xử lý
            while (true)
            {
                var orders = await _orderService.GetOrdersForAdminAsync(currentFilter);

                var menuContent = CreateMenu(currentFilter);
                var viewContent = CreateOrderTable(orders);
                var notification = new Markup("[dim]Chọn bộ lọc hoặc nhập 'v.{id}' để xem chi tiết.[/]");

                _layout.Render(menuContent, viewContent, notification);

                var choice = AnsiConsole.Ask<string>("\n> Nhập lựa chọn:").ToLower();

                if (choice.StartsWith("v."))
                {
                    if (int.TryParse(choice.AsSpan(2), out int orderId))
                    {
                        await HandleViewOrderDetailsAsync(orderId);
                    }
                    continue; // Quay lại vòng lặp để làm mới danh sách
                }

                switch (choice)
                {
                    case "1": currentFilter = "ActionRequired"; break;
                    case "2": currentFilter = null; break; // null = Tất cả
                    case "3": currentFilter = "Processing"; break;
                    case "4": currentFilter = "Cancelled"; break;
                    case "0": return;
                    default: break;
                }
            }
        }

        private Markup CreateMenu(string? activeFilter)
        {
            var menuItems = new Dictionary<string, string>
            {
                { "1", "Đơn hàng cần xử lý" },
                { "2", "Xem Tất cả" },
                { "3", "Đơn hàng đã xác nhận" },
                { "4", "Đơn hàng đã hủy" }
            };

            var sb = new System.Text.StringBuilder();
            sb.AppendLine("[bold yellow underline]QUẢN LÝ ĐƠN HÀNG[/]");
            sb.AppendLine("[bold]Lọc theo trạng thái:[/]");

            foreach (var item in menuItems)
            {
                // Highlight bộ lọc đang được chọn
                if ((activeFilter == "ActionRequired" && item.Key == "1") || activeFilter == null && item.Key == "2" || activeFilter == "Processing" && item.Key == "3" || activeFilter == "Cancelled" && item.Key == "4")
                {
                    sb.AppendLine($" [bold yellow]>[/] [underline yellow][{item.Key}] {item.Value}[/]");
                }
                else
                {
                    sb.AppendLine($"   [{item.Key}] {item.Value}");
                }
            }

            sb.AppendLine("\n[red][0] Quay lại[/]");
            return new Markup(sb.ToString());
        }

        private Table CreateOrderTable(List<Order> orders)
        {
            var table = new Table().Expand().Border(TableBorder.Rounded);
            table.AddColumn("ID Đơn hàng");
            table.AddColumn("Mã Đơn");
            table.AddColumn("Tên Khách hàng");
            table.AddColumn("Ngày đặt");
            table.AddColumn("Tổng tiền");
            table.AddColumn("Trạng thái");

            if (!orders.Any())
            {
                table.AddRow("[grey]Không có đơn hàng nào phù hợp.[/]", "", "", "", "", "");
                return table;
            }

            foreach (var order in orders)
            {
                table.AddRow(
            new Markup(order.OrderID.ToString()),
            new Markup(Markup.Escape(order.OrderCode)),
            new Markup(Markup.Escape(order.User.FullName)),
            new Markup(order.OrderDate.ToString("dd/MM/yyyy HH:mm")),
            new Markup($"[yellow]{order.TotalPrice:N0} VNĐ[/]"),
            FormatOrderStatus(order.Status)
        );
            }
            return table;
        }

        private async Task HandleViewOrderDetailsAsync(int orderId)
        {
            var order = await _orderService.GetOrderByIdAsync(orderId, 0); // Lấy chi tiết đơn hàng cho admin
            if (order == null)
            {
                AnsiConsole.MarkupLine("[red]Không tìm thấy đơn hàng với ID này.[/]");
                Console.ReadKey();
                return;
            }

            while (true)
            {
                AnsiConsole.Clear();
                DisplayOrderDetails(order);

                var actionMenu = CreateContextualActionMenu(order.Status);
                if (actionMenu == null) // Không có hành động nào
                {
                    AnsiConsole.MarkupLine("\n[dim]Đơn hàng này đã được xử lý. Nhấn phím bất kỳ để quay lại.[/]");
                    Console.ReadKey();
                    return;
                }

                AnsiConsole.Write(actionMenu);
                var choice = AnsiConsole.Ask<string>("\n> Chọn hành động cho đơn hàng này:").ToLower();
                var adminId = _sessionService.CurrentUser.UserID;

                bool shouldReturn = false;
                switch (order.Status)
                {
                    case "PendingAdminApproval":
                        if (choice == "1") await HandleConfirmOrder(orderId, adminId);
                        else if (choice == "2") await HandleRejectOrder(orderId, adminId);
                        if (choice == "1" || choice == "2") shouldReturn = true;
                        break;

                    case "CancellationRequested":
                        if (choice == "1") await HandleApproveCancellation(orderId, adminId);
                        if (choice == "1") shouldReturn = true;
                        break;
                }

                if (choice == "0") shouldReturn = true;
                if (shouldReturn) return;
            }
        }

        #region Action Handlers & Helpers
        private async Task HandleConfirmOrder(int orderId, int adminId)
        {
            var response = await _orderService.ConfirmOrderAsync(orderId, adminId);
            string color = response.IsSuccess ? "green" : "red";
            AnsiConsole.MarkupLine($"\n[{color}]{Markup.Escape(response.Message)}[/]");
            Console.ReadKey();
        }

        private async Task HandleRejectOrder(int orderId, int adminId)
        {
            var reason = AnsiConsole.Ask<string>("Nhập [green]lý do từ chối[/] đơn hàng:");
            if (string.IsNullOrWhiteSpace(reason))
            {
                AnsiConsole.MarkupLine("[red]Lý do không được để trống.[/]");
                Console.ReadKey();
                return;
            }
            var response = await _orderService.RejectOrderAsync(orderId, adminId, reason);
            string color = response.IsSuccess ? "green" : "red";
            AnsiConsole.MarkupLine($"\n[{color}]{Markup.Escape(response.Message)}[/]");
            Console.ReadKey();
        }

        private async Task HandleApproveCancellation(int orderId, int adminId)
        {
            var response = await _orderService.ApproveCancellationAsync(orderId, adminId);
            string color = response.IsSuccess ? "green" : "red";
            AnsiConsole.MarkupLine($"\n[{color}]{Markup.Escape(response.Message)}[/]");
            Console.ReadKey();
        }

        private Panel? CreateContextualActionMenu(string status)
        {
            var grid = new Grid().AddColumn();
            bool hasAction = false;

            if (status == "PendingAdminApproval")
            {
                grid.AddRow("[green][1] Xác nhận đơn hàng[/]");
                grid.AddRow("[red][2] Từ chối đơn hàng[/]");
                hasAction = true;
            }
            else if (status == "CancellationRequested")
            {
                grid.AddRow("[green][1] Chấp thuận yêu cầu hủy[/]");
                hasAction = true;
            }

            if (!hasAction) return null;

            grid.AddRow("[grey][0] Quay lại danh sách[/]");
            return new Panel(grid).Header(new PanelHeader("[yellow]HÀNH ĐỘNG[/]").Centered());
        }

        private Markup FormatOrderStatus(string status)
        {
            return status switch
            {
                "PendingAdminApproval" => new Markup("[yellow]WAIT[/]"),
                "Processing" => new Markup("[green]DONE[/]"),
                "Cancelled" => new Markup("[red]CANCELLED[/]"),
                "RejectedByAdmin" => new Markup("[red]CANCELLED[/]"),
                "CustomerCancelled" => new Markup("[red]CANCELLED[/]"),
                "CancellationRequested" => new Markup("[orange1]REQ_CANCEL[/]"),
                _ => new Markup(Markup.Escape(status))
            };
        }

        private void DisplayOrderDetails(Order order)
        {
            // Panel thông tin chung
            var infoPanel = new Panel(
                new Grid()
                    .AddColumn().AddColumn()
                    .AddRow("[bold]Mã đơn:[/]", Markup.Escape(order.OrderCode))
                    .AddRow("[bold]Khách hàng:[/]", Markup.Escape(order.User.FullName))
                    .AddRow("[bold]Ngày đặt:[/]", order.OrderDate.ToString("g"))
                    .AddRow("[bold]Trạng thái:[/]", FormatOrderStatus(order.Status).ToString())
                    .AddRow("[bold]Địa chỉ giao:[/]", Markup.Escape(order.ShippingAddress))
                    .AddRow("[bold]SĐT Nhận:[/]", Markup.Escape(order.ShippingPhone))
            )
            .Header($"CHI TIẾT ĐƠN HÀNG - ID: {order.OrderID}")
            .Expand();
            AnsiConsole.Write(infoPanel);

            // Bảng chi tiết sản phẩm
            var productTable = new Table().Expand().Border(TableBorder.Rounded);
            productTable.AddColumn("Sản phẩm");
            productTable.AddColumn("Size");
            productTable.AddColumn("SL");
            productTable.AddColumn("Đơn giá");
            productTable.AddColumn("Thành tiền");

            foreach (var detail in order.OrderDetails)
            {
                productTable.AddRow(
                    Markup.Escape(detail.Product.Name),
                    detail.Size.ToString(),
                    detail.Quantity.ToString(),
                    $"{detail.UnitPrice:N0} VNĐ",
                    $"[bold]{(detail.UnitPrice * detail.Quantity):N0} VNĐ[/]"
                );
            }
            AnsiConsole.Write(productTable);
            AnsiConsole.MarkupLine($"\n[bold yellow]TỔNG TIỀN THANH TOÁN: {order.TotalPrice:N0} VNĐ[/]");

            if (!string.IsNullOrEmpty(order.CustomerCancellationReason))
            {
                AnsiConsole.MarkupLine($"[bold orange1]Lý do khách hủy:[/] [italic]{Markup.Escape(order.CustomerCancellationReason)}[/]");
            }
        }
        #endregion
    }
}