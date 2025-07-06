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
    public class AdminCustomerMenu : IAdminCustomerMenu
    {
        private readonly IUserService _userService;
        private readonly IOrderService _orderService;
        private readonly ConsoleLayout _layout;

        private class CustomerListState
        {
            public int CurrentPage { get; set; } = 1;
            public int PageSize { get; set; } = 10;
            public int TotalPages { get; set; } = 1;
            public string SortBy { get; set; } = "spending_desc";
        }

        public AdminCustomerMenu(IUserService userService, IOrderService orderService, ConsoleLayout layout)
        {
            _userService = userService;
            _orderService = orderService;
            _layout = layout;
        }

        public async Task ShowAsync()
        {
            var state = new CustomerListState();
            while (true)
            {
                var (customers, totalPages) = await _userService.GetCustomerStatisticsAsync(state.SortBy, state.CurrentPage, state.PageSize);
                state.TotalPages = totalPages;

                var menuContent = CreateSideMenu();
                var viewContent = CreateCustomerTable(customers);
                var notificationContent = new Markup($"Trang [bold yellow]{state.CurrentPage}[/] / [bold yellow]{state.TotalPages}[/]. " +
                                                     "[dim]Chọn sắp xếp hoặc dùng lệnh '[/][blue]cus.{id}[/][dim]' để xem chi tiết.[/]");

                _layout.Render(menuContent, viewContent, notificationContent);

                var choice = AnsiConsole.Ask<string>("\n> Nhập lựa chọn:").ToLower();

                if (await HandleCommand(choice, state) == false) return;
            }
        }

        private async Task<bool> HandleCommand(string choice, CustomerListState state)
        {
            if (choice.StartsWith("cus."))
            {
                if (int.TryParse(choice.AsSpan(4), out int customerId))
                {
                    await ShowCustomerDetailsAsync(customerId);
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

            var sortChanged = true;
            switch (choice)
            {
                case "1": state.SortBy = "spending_desc"; break;
                case "2": state.SortBy = "spending_asc"; break;
                case "n":
                    if (state.CurrentPage < state.TotalPages) state.CurrentPage++;
                    sortChanged = false;
                    break;
                case "p":
                    if (state.CurrentPage > 1) state.CurrentPage--;
                    sortChanged = false;
                    break;
                case "0": return false;
                default:
                    sortChanged = false;
                    break;
            }

            if (sortChanged) state.CurrentPage = 1;
            return true;
        }

        private async Task ShowCustomerDetailsAsync(int customerId)
        {
            int currentPage = 1;
            const int pageSize = 5;
            int totalPages = 1;

            while (true)
            {
                var (customerOrders, calculatedTotalPages) = await _orderService.GetOrdersAsync(customerId, "Processing|Completed", currentPage, pageSize);
                totalPages = calculatedTotalPages;
                var customerInfo = customerOrders.FirstOrDefault()?.User ?? (await _userService.GetCustomerStatisticsAsync("default", 1, 1)).Customers.FirstOrDefault(u => u.UserID == customerId);


                if (customerInfo == null)
                {
                    AnsiConsole.MarkupLine("[red]Không tìm thấy thông tin khách hàng.[/]");
                    Console.ReadKey();
                    return;
                }

                var menuContent = CreateCustomerInfoMenu(customerInfo);
                var viewContent = CreateCustomerOrderTable(customerOrders);
                var notificationContent = new Markup($"Trang [bold yellow]{currentPage}[/] / [bold yellow]{totalPages}[/]. " +
                                                     "[dim]Nhập '[/][blue]or.{id}[/][dim]' để xem chi tiết, hoặc '[/][red]0[/][dim]' để quay lại.[/]");

                _layout.Render(menuContent, viewContent, notificationContent);

                var choice = AnsiConsole.Ask<string>("\n> Nhập lựa chọn:").ToLower();

                if (choice == "0") return;

                if (choice.StartsWith("p."))
                {
                    if (int.TryParse(choice.AsSpan(2), out int page) && page > 0 && page <= totalPages)
                    {
                        currentPage = page;
                    }
                    continue;
                }

                if (choice.StartsWith("n"))
                {
                    if (currentPage < totalPages) currentPage++;
                    continue;
                }

                if (choice.StartsWith("p"))
                {
                    if (currentPage > 1) currentPage--;
                    continue;
                }

                if (choice.StartsWith("or."))
                {
                    if (int.TryParse(choice.AsSpan(3), out int orderId))
                    {
                        await HandleViewOrderDetailsAsync(orderId);
                        continue;
                    }
                }
            }
        }
        #region UI Creation & Helper Methods

        private Markup CreateSideMenu()
        {
            return new Markup(
                "[bold yellow underline]QUẢN LÝ KHÁCH HÀNG[/]\n\n" +
                "[bold]Sắp xếp theo:[/]\n" +
                " 1. Cao-Thấp\n" +
                " 2. Thấp-Cao\n\n" +
                "[bold]Hành động:[/]\n" +
                " cus.{id} - Xem chi tiết khách hàng\n\n" +
                " [red]0. Quay lại[/]"
            );
        }

        private Table CreateCustomerTable(List<User> customers)
        {
            var table = new Table().Expand().Border(TableBorder.Rounded);
            table.Title = new TableTitle("[yellow]THỐNG KÊ KHÁCH HÀNG[/]");
            table.AddColumn("ID Khách");
            table.AddColumn("Tên Khách hàng");
            table.AddColumn("Số đơn đã mua");
            table.AddColumn("Tổng chi tiêu");

            foreach (var customer in customers)
            {
                // Đếm các đơn hàng đã được duyệt
                int approvedOrders = customer.Orders.Count(o => o.Status == "Processing" || o.Status == "Completed");
                table.AddRow(
                    new Markup(Markup.Escape(customer.UserID.ToString())),
                    new Markup(Markup.Escape(customer.FullName)),
                    new Markup($"[cyan]{approvedOrders}[/]"),
                    new Markup($"[bold yellow]{customer.TotalSpending:N0} VNĐ[/]")
                );
            }
            return table;
        }

        private Markup CreateCustomerInfoMenu(User customer)
        {
            return new Markup(
               $"[bold yellow underline]CHI TIẾT KHÁCH HÀNG[/]\n\n" +
               $"[bold]Tên:[/] {Markup.Escape(customer.FullName)}\n" +
               $"[bold]Email:[/] {Markup.Escape(customer.Email)}\n" +
               $"[bold]SĐT:[/] {Markup.Escape(customer.PhoneNumber)}\n" +
               $"[bold]Gender:[/] {Markup.Escape(customer.Gender)}\n\n" +
               "[red]0. Quay lại danh sách[/]"
           );
        }

        private Table CreateCustomerOrderTable(List<Order> orders)
        {
            var table = new Table().Expand().Border(TableBorder.Rounded);
            table.Title = new TableTitle("[yellow]Lịch sử đơn hàng đã duyệt[/]");
            table.AddColumn("ID Đơn hàng");
            table.AddColumn("Mã Đơn");
            table.AddColumn("Ngày đặt");
            table.AddColumn("Tổng tiền");

            if (!orders.Any())
            {
                table.AddRow("[grey]Khách hàng này chưa có đơn hàng nào được duyệt.[/]", "", "", "");
                return table;
            }

            foreach (var order in orders)
            {
                table.AddRow(
                    new Markup(Markup.Escape(order.OrderID.ToString())),
                    new Markup(Markup.Escape(order.OrderCode)),
                    new Markup(Markup.Escape(order.OrderDate.ToString("dd/MM/yyyy"))),
                    new Markup($"[yellow]{order.TotalPrice:N0} VNĐ[/]")
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
            AnsiConsole.MarkupLine("\n[dim]Nhấn phím bất kỳ để quay lại danh sách...[/]");
            Console.ReadKey();
        }

        private void DisplayOrderDetails(Order order)
        {
            var infoPanel = new Panel(
                new Grid()
                    .AddColumn().AddColumn()
                    .AddRow(new Markup("[bold]Mã đơn:[/]"), new Markup(Markup.Escape(order.OrderCode)))
                    .AddRow(new Markup("[bold]Khách hàng:[/]"), new Markup(Markup.Escape(order.User?.FullName ?? "N/A")))
                    .AddRow(new Markup("[bold]Ngày đặt:[/]"), new Markup(Markup.Escape(order.OrderDate.ToString("g"))))
                    .AddRow(new Markup("[bold]Trạng thái:[/]"), FormatOrderStatus(order.Status))
                    .AddRow(new Markup("[bold]Địa chỉ giao:[/]"), new Markup(Markup.Escape(order.ShippingAddress)))
                    .AddRow(new Markup("[bold]SĐT Nhận:[/]"), new Markup(Markup.Escape(order.ShippingPhone)))
            )
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
        private Markup FormatOrderStatus(string? status)
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
                _ => new Markup(Markup.Escape(status ?? "N/A"))
            };
        }

        #endregion
    }
}