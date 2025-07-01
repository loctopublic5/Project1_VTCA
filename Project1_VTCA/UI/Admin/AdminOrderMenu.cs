using Project1_VTCA.Data;
using Project1_VTCA.Services.Interface;
using Project1_VTCA.UI.Admin.Interface;
using Project1_VTCA.UI.Draw;
using Spectre.Console;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
            string? currentFilter = "ActionRequired";
            int currentPage = 1;
            const int pageSize = 10;

            while (true)
            {
                var (orders, totalPages) = await _orderService.GetOrdersForAdminAsync(currentFilter, currentPage, pageSize);
                if (currentPage > totalPages && totalPages > 0)
                {
                    currentPage = totalPages;
                }

                var menuContent = CreateSideMenu();
                var viewContent = CreateOrderTable(orders);
                var notificationContent = new Markup($"Trang [bold yellow]{currentPage}[/] / [bold yellow]{totalPages}[/]. " +
                                                     "[dim]Chọn bộ lọc hoặc dùng lệnh hành động.[/]");

                _layout.Render(menuContent, viewContent, notificationContent);

                var choice = AnsiConsole.Ask<string>("\n> Nhập lựa chọn:").ToLower();

                if (choice.StartsWith("v."))
                {
                    if (int.TryParse(choice.AsSpan(2), out int orderId))
                    {
                        // Logic "Xem chi tiết theo Ngữ cảnh"
                        if (orders.Any(o => o.OrderID == orderId))
                        {
                            await HandleViewOrderDetailsAsync(orderId);
                        }
                        else
                        {
                            AnsiConsole.MarkupLine("[red]Lỗi: ID đơn hàng không hợp lệ hoặc không thuộc bộ lọc/trang hiện tại.[/]");
                            Console.ReadKey();
                        }
                    }
                    continue;
                }

                bool filterChanged = true;
                switch (choice)
                {
                    case "1": currentFilter = "ActionRequired"; break;
                    case "2": currentFilter = null; break;
                    case "3": currentFilter = "Processing"; break;
                    case "4": currentFilter = "Cancelled"; break;
                    case "n":
                        if (currentPage < totalPages) currentPage++;
                        filterChanged = false;
                        break;
                    case "p":
                        if (currentPage > 1) currentPage--;
                        filterChanged = false;
                        break;
                    case "0": return;
                    default:
                        filterChanged = false;
                        break;
                }
                if (filterChanged)
                {
                    currentPage = 1;
                }
            }
        }

        private Markup CreateSideMenu()
        {
            // Menu tĩnh, an toàn và không thay đổi
            return new Markup(
                "[bold yellow underline]QUẢN LÝ ĐƠN HÀNG[/]\n\n" +
                "[bold green underline]Lọc theo trạng thái:[/]\n" +
                " 1. Đơn hàng cần xử lý\n" +
                " 2. Xem Tất cả\n" +
                " 3. Đơn hàng đã xác nhận\n" +
                " 4. Đơn hàng đã hủy\n\n" +
                "[bold green underline]Hành động & Điều hướng:[/]\n" +
                " [dim]v.{id} - Xem chi tiết\n" +
                " n - Trang sau, p - Trang trước[/]\n" +
                " [red]0. Quay lại[/]"
            );
        }

        private Table CreateOrderTable(List<Order> orders)
        {
            var table = new Table().Expand().Border(TableBorder.Rounded);
            table.Title = new TableTitle("DANH SÁCH ĐƠN HÀNG");
            table.AddColumn("ID");
            table.AddColumn("Mã Đơn");
            table.AddColumn("Khách hàng");
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
            new Markup(Markup.Escape(order.OrderID.ToString())),
            new Markup(Markup.Escape(order.OrderCode)),
            new Markup(Markup.Escape(order.User?.FullName ?? "N/A")),
            new Markup(Markup.Escape(order.OrderDate.ToString("dd/MM/yyyy HH:mm"))),
            new Markup($"[yellow]{order.TotalPrice:N0} VNĐ[/]"),
            FormatOrderStatus(order.Status)
        );
            }
            return table;
        }

        private async Task HandleViewOrderDetailsAsync(int orderId)
        {
            var order = await _orderService.GetOrderByIdAsync(orderId, 0);
            if (order == null)
            {
                AnsiConsole.MarkupLine("[red]Lỗi: Không tìm thấy đơn hàng với ID này.[/]");
                Console.ReadKey();
                return;
            }

            AnsiConsole.Clear();
            DisplayOrderDetails(order);

            var actionPrompt = CreateContextualActionPrompt(order.Status);
            if (actionPrompt == null)
            {
                AnsiConsole.MarkupLine("\n[dim]Đơn hàng này đã được xử lý. Nhấn phím bất kỳ để quay lại.[/]");
                Console.ReadKey();
                return;
            }

            var actionChoice = AnsiConsole.Prompt(actionPrompt);
            if (actionChoice == "Quay lại") return;

            var adminId = _sessionService.CurrentUser.UserID;
            ServiceResponse response = new(false, "Hành động không xác định.");

            switch (actionChoice)
            {
                case "Xác nhận đơn hàng":
                    response = await _orderService.ConfirmOrderAsync(orderId, adminId);
                    break;
                case "Từ chối đơn hàng":
                    var reason = AnsiConsole.Ask<string>("Nhập [green]lý do từ chối[/]:");
                    if (!string.IsNullOrWhiteSpace(reason))
                    {
                        response = await _orderService.RejectOrderAsync(orderId, adminId, reason);
                    }
                    else
                    {
                        AnsiConsole.MarkupLine("[red]Lý do không được để trống.[/]");
                        Console.ReadKey();
                    }
                    break;
                case "Chấp thuận yêu cầu hủy":
                    response = await _orderService.ApproveCancellationAsync(orderId, adminId);
                    break;
            }

            AnsiConsole.MarkupLine($"\n[{(response.IsSuccess ? "green" : "red")}]{Markup.Escape(response.Message)}[/]");
            Console.ReadKey();
        }

        #region Helper Methods

        private SelectionPrompt<string>? CreateContextualActionPrompt(string? status)
        {
            var prompt = new SelectionPrompt<string>().Title("[bold yellow]Hành động có sẵn cho đơn hàng này:[_]");
            switch (status)
            {
                case "PendingAdminApproval":
                    prompt.AddChoices("Xác nhận đơn hàng", "Từ chối đơn hàng", "Quay lại");
                    return prompt;
                case "CancellationRequested":
                    prompt.AddChoices("Chấp thuận yêu cầu hủy", "Quay lại");
                    return prompt;
                default:
                    return null;
            }
        }

        private Markup FormatOrderStatus(string? status)
        {
            return status switch
            {
                "PendingAdminApproval" => new Markup("[yellow]WAITING[/]"),
                "Processing" => new Markup("[green]PROCESSING[/]"),
                "Completed" => new Markup("[blue]COMPLETED[/]"),
                "RejectedByAdmin" => new Markup("[red]REJECTED[/]"),
                "CustomerCancelled" => new Markup("[red]CANCELLED[/]"),
                "CancellationRequested" => new Markup("[orange1]REQ_CANCEL[/]"),
                _ => new Markup(Markup.Escape(status ?? "N/A"))
            };
        }

        private void DisplayOrderDetails(Order order)
        {
            var infoGrid = new Grid();
            infoGrid.AddColumn();
            infoGrid.AddColumn();
            infoGrid.AddRow("[bold]Mã đơn:[/]", Markup.Escape(order.OrderCode));
            infoGrid.AddRow("[bold]Khách hàng:[/]", Markup.Escape(order.User?.FullName ?? "N/A"));
            infoGrid.AddRow("[bold]Ngày đặt:[/]", Markup.Escape(order.OrderDate.ToString("g")));
            infoGrid.AddRow("[bold]Trạng thái:[/]", Markup.Escape(FormatOrderStatus(order.Status).ToString()));
            infoGrid.AddRow("[bold]Địa chỉ giao:[/]", Markup.Escape(order.ShippingAddress));
            infoGrid.AddRow("[bold]SĐT Nhận:[/]", Markup.Escape(order.ShippingPhone));

            var infoPanel = new Panel(infoGrid)
                .Header($"CHI TIẾT ĐƠN HÀNG - ID: {order.OrderID}")
                .Expand();
            AnsiConsole.Write(infoPanel);

            var productTable = new Table().Expand().Border(TableBorder.Rounded);
            productTable.Title = new TableTitle("Sản phẩm trong đơn");
            productTable.AddColumn("Sản phẩm");
            productTable.AddColumn("Size");
            productTable.AddColumn("SL");
            productTable.AddColumn("Đơn giá");
            productTable.AddColumn("Thành tiền");

            foreach (var detail in order.OrderDetails)
            {
                productTable.AddRow(
                    Markup.Escape(detail.Product.Name),
                    Markup.Escape(detail.Size.ToString()),
                    Markup.Escape(detail.Quantity.ToString()),
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
            if (!string.IsNullOrEmpty(order.AdminDecisionReason))
            {
                AnsiConsole.MarkupLine($"[bold red]Lý do Admin từ chối:[/] [italic]{Markup.Escape(order.AdminDecisionReason)}[/]");
            }
        }
        #endregion
    }
}