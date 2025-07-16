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
        private const string SELECT_ALL_CHOICE = "[bold yellow](Chọn/Bỏ chọn Tất cả các đơn trong lô này)[/]";
        private const string REASON_OUT_OF_STOCK = "Sản phẩm hiện đang hết hàng";
        private const string REASON_OTHER = "Lý do khác (nhập chi tiết)...";

        public AdminOrderMenu(IOrderService orderService, ISessionService sessionService, ConsoleLayout layout)
        {
            _orderService = orderService;
            _sessionService = sessionService;
            _layout = layout;
        }

        private class AdminOrderState
        {
            public int CurrentPage { get; set; } = 1;
            public int PageSize { get; set; } = 10;
            public int TotalPages { get; set; }
            public int TotalOrdersInFilter { get; set; } 
            public string? StatusFilter { get; set; } = "PendingAdminApproval";
        }
        public async Task ShowAsync()
        {
            while (true)
            {
               var state = new AdminOrderState();
                var (orders, totalPages, totalCount) = await _orderService.GetOrdersForAdminAsync(state.StatusFilter, state.CurrentPage, state.PageSize);
                state.TotalPages = totalPages;
                state.TotalOrdersInFilter = totalCount; 

                if (state.CurrentPage > state.TotalPages && state.TotalPages > 0)
                {
                    state.CurrentPage = state.TotalPages;
                }
                var menuContent = CreateMainMenu();
                var viewContent = CreateOrderTable(orders, state.StatusFilter);
                var notificationContent = CreateNotificationPanel(state);

                _layout.Render(menuContent, viewContent, notificationContent);

                var choice = AnsiConsole.Ask<string>("\n> Nhập lựa chọn:").ToLower();

                switch (choice)
                {
                    case "1":
                        await HandleBulkConfirmFlowAsync();
                        break;
                    case "2":
                        await HandleBulkRejectFlowAsync();
                        Console.ReadKey();
                        break;
                    case "3":
                        await HandleFilterAndViewOrdersAsync();
                        break;
                    case "0":
                        return;
                }

            }
        }

        private Markup CreateMainMenu()
        {
            return new Markup(
                "[bold yellow underline]TRUNG TÂM ĐIỀU HÀNH[/]\n\n" +
                "[bold]Xử lý hàng loạt:[/]\n" +
                " 1. Xác nhận Đơn hàng\n" +
                " 2. Hủy Đơn hàng\n" +
                "[bold]Tra cứu:[/]\n" +
                " 3. Lọc và Xem đơn hàng\n\n" +
                " [red]0. Quay lại[/]"
            );
        }
        private Table CreateOrderTable(List<Order> orders, string? filter)
        {
            var title = filter switch
            {
                "PendingAdminApproval" => "Danh sách Đơn hàng Chờ Duyệt",
                "Processing" => "Danh sách Đơn hàng Đã Duyệt",
                "Cancelled" => "Danh sách Đơn hàng Đã Hủy",
                _ => "Toàn bộ Đơn hàng"
            };

            var table = new Table().Expand().Border(TableBorder.Rounded);
            table.Title = new TableTitle($"[bold yellow]{Markup.Escape(title)}[/]");
            table.AddColumn("ID");
            table.AddColumn("Mã Đơn");
            table.AddColumn("Khách hàng");
            table.AddColumn("Ngày đặt");
            table.AddColumn("Tổng Giá");
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
            new Markup(Markup.Escape(order.OrderDate.ToString("HH:mm dd/MM/yy"))),
            new Markup($"[yellow]{order.TotalPrice:N0} VNĐ[/]"),
            FormatOrderStatus(order.Status)
        );
            }
            return table;
        }

        private Markup CreateNotificationPanel(AdminOrderState state)
        {
            var notificationText = new StringBuilder();
            notificationText.Append($"Trang [bold yellow]{state.CurrentPage}[/] / [bold yellow]{state.TotalPages}[/]. ");

            if (state.StatusFilter == "PendingAdminApproval")
            {
                if (state.TotalOrdersInFilter > 0)
                    notificationText.Append($"[yellow]Thông báo:[/] Có [bold]{state.TotalOrdersInFilter}[/] đơn hàng mới đang chờ bạn duyệt.");
                else
                    notificationText.Append("[green]Thông báo:[/] Không có đơn hàng nào cần xử lý. Rất tốt!");
            }
            else
            {
                notificationText.Append("[dim]Chọn bộ lọc hoặc dùng lệnh hành động.[/]");
            }

            return new Markup(notificationText.ToString());
        }

        #region Lọc và Xem đơn hàng
        private async Task HandleFilterAndViewOrdersAsync()
        {
            string? currentFilter = null;
            int currentPage = 1;
            const int pageSize = 10;
            int totalPages = 1;

            while (true)
            {
                var (orders, calculatedTotalPages, totalCount) = await _orderService.GetOrdersForAdminAsync(currentFilter, currentPage, pageSize);

                totalPages = calculatedTotalPages;
                if (currentPage > totalPages && totalPages > 0)
                {
                    currentPage = totalPages;
                }

                var menuContent = CreateFilterSideMenu();
                var viewContent = CreateDetailedOrderTable(orders);
                var notificationContent = new Markup($"Trang [bold yellow]{currentPage}[/] / [bold yellow]{totalPages}[/]. " +
                                                     "[dim]Chọn bộ lọc hoặc dùng lệnh điều hướng.[/]");

                _layout.Render(menuContent, viewContent, notificationContent);

                var choice = AnsiConsole.Ask<string>("\n> Nhập lựa chọn:").ToLower();

                if (choice == "0") break;


                if (choice.StartsWith("p."))
                {
                    if (int.TryParse(choice.AsSpan(2), out int page) && page > 0 && page <= totalPages)
                    {
                        currentPage = page;
                    }
                    continue;
                }

                else if (choice.StartsWith("or."))
                {
                    if (int.TryParse(choice.AsSpan(3), out int orderId))
                    {
                        await HandleViewOrderDetailsAsync(orderId);
                    }
                    else
                    {
                        AnsiConsole.MarkupLine("[red]Lỗi: Cú pháp lệnh không hợp lệ.[/]");
                        Console.ReadKey();
                    }
                    continue;
                }

                var filterChanged = true;
                switch (choice)
                {
                    case "1": currentFilter = null; break;
                    case "2": currentFilter = "PendingAdminApproval"; break;
                    case "3": currentFilter = "Processing|Completed"; break;
                    case "4": currentFilter = "CustomerCancelled|RejectedByAdmin"; break;
                    case "n":
                        if (currentPage < totalPages) currentPage++;
                        filterChanged = false;
                        break;
                    case "p":
                        if (currentPage > 1) currentPage--;
                        filterChanged = false;
                        break;

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

        private Markup CreateFilterSideMenu()
        {
            return new Markup(
                "[bold yellow underline]LỌC ĐƠN HÀNG[/]\n\n" +
                "[bold]Lọc theo trạng thái:[/]\n" +
                " 1. Xem Tất cả\n" +
                " 2. Đơn hàng Chờ duyệt\n" +
                " 3. Đơn hàng Đã duyệt\n" +
                " 4. Đơn hàng Đã hủy\n\n" +
                "[bold]Điều hướng:[/]\n" +
                "[dim] n - Trang sau\n" +
                " p - Trang trước\n" +
                " P.{số trang} - Đến trang bất kì\n" +
                " or.{id} - xem chi tiết đơn hàng[/]\n\n" +
                " [red]0. Quay lại[/]"
            );
        }
        #endregion

        #region xác nhận đơn hàng
        private async Task HandleBulkConfirmFlowAsync()
        {
            const int batchSize = 10;
            while (true)
            {
                var (batch, _, totalCount) = await _orderService.GetOrdersForAdminAsync("PendingAdminApproval", 1, batchSize);
                if (!batch.Any())
                {
                    AnsiConsole.MarkupLine("\n[green]Tuyệt vời! Đã xử lý xong tất cả các đơn hàng đang chờ xác nhận.[/]");
                    Console.ReadKey();
                    break;
                }

                var menuContent = new Markup("[bold yellow]ĐANG XỬ LÝ:\nXÁC NHẬN ĐƠN HÀNG[/]");
                var viewContent = CreateDetailedOrderTable(batch);
                var notificationContent = new Markup("[dim]Dùng danh sách bên dưới để chọn.[/]");
                _layout.Render(menuContent, viewContent, notificationContent);

                var prompt = CreateMultiSelectPrompt(batch, "xác nhận");
                var selectedItems = AnsiConsole.Prompt(prompt);
                var finalSelection = ProcessSelection(selectedItems, batch);

                if (!finalSelection.Any())
                {
                    if (AnsiConsole.Confirm("[yellow]Bạn không chọn đơn hàng nào. Bạn có muốn thoát khỏi chức năng này không?[/]")) break;
                    continue;
                }

         
                if (AnsiConsole.Confirm($"\n[yellow]Bạn có chắc chắn muốn xác nhận [bold]{finalSelection.Count}[/] đơn hàng đã chọn không?[/]"))
                {
                    var orderIds = finalSelection.Select(o => o.OrderID).ToList();
                    var adminId = _sessionService.CurrentUser.UserID;
                  
                    await ProcessOrdersIndividuallyAndReport(finalSelection, adminId);
                }
                else
                {
                    AnsiConsole.MarkupLine("[yellow]Đã hủy thao tác.[/]");
                }

                var (remainingOrders, _, _) = await _orderService.GetOrdersForAdminAsync("PendingAdminApproval", 1, 1);
                if (remainingOrders.Any())
                {
                    if (!AnsiConsole.Confirm("[cyan]Vẫn còn đơn hàng cần xử lý. Bạn có muốn tiếp tục không?[/]"))
                    {
                        break;
                    }
                }
                else
                {
                    AnsiConsole.MarkupLine("\n[green]Đã xử lý xong tất cả đơn hàng.[/]");
                    Console.ReadKey();
                    break;
                }
            }
        }

        #endregion 

        #region huỷ đơn hàng phía admin
        private async Task HandleBulkRejectFlowAsync()
        {
            const int batchSize = 10;
            while (true)
            {
                var (batch, totalPages, totalCount) = await _orderService.GetOrdersForAdminAsync("PendingAdminApproval", 1, batchSize);

                if (!batch.Any())
                {
                    AnsiConsole.MarkupLine("\n[green]Không còn đơn hàng nào đang chờ để hủy.[/]");
                    Console.ReadKey();
                    break;
                }

                var prompt = CreateMultiSelectPrompt(batch, "hủy");
                var selectedItems = AnsiConsole.Prompt(prompt);
                var finalSelection = ProcessSelection(selectedItems, batch);

                if (!finalSelection.Any())
                {
                    if (AnsiConsole.Confirm("[yellow]Bạn không chọn đơn hàng nào. Bạn có muốn thoát không?[/]")) break;
                    continue;
                }

                var reason = GetRejectionReason();
                if (reason == null)
                {
                    if (AnsiConsole.Confirm("[yellow]Đã hủy bước chọn lý do. Bạn có muốn thoát khỏi chức năng này không?[/]")) break;
                    continue;
                }

                
                if (AnsiConsole.Confirm($"\n[yellow]Bạn có chắc chắn muốn hủy [bold]{finalSelection.Count}[/] đơn hàng với lý do \"[orange1]{Markup.Escape(reason)}[/]\" không?[/]"))
                {
                    var orderIds = finalSelection.Select(o => o.OrderID).ToList();
                    var adminId = _sessionService.CurrentUser.UserID;
                    var response = await _orderService.BulkRejectOrdersAsync(orderIds, adminId, reason);

                    AnsiConsole.MarkupLine($"\n[{(response.IsSuccess ? "green" : "red")}]{Markup.Escape(response.Message)}[/]");
                }
                else
                {
                    AnsiConsole.MarkupLine("[yellow]Đã hủy thao tác.[/]");
                }

                var (remainingOrders, _, _) = await _orderService.GetOrdersForAdminAsync("PendingAdminApproval", 1, 1);
                if (remainingOrders.Any())
                {
                    if (!AnsiConsole.Confirm("[cyan]Vẫn còn đơn hàng cần xử lý. Bạn có muốn tiếp tục không?[/]"))
                    {
                        break;
                    }
                }
                else
                {
                    AnsiConsole.MarkupLine("\n[green]Đã xử lý xong tất cả đơn hàng.[/]");
                    Console.ReadKey();
                    break;
                }
            }
        }

        private string? GetRejectionReason()
        {
            var reasonChoice = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("\nChọn [green]lý do hủy[/] chung cho các đơn hàng đã chọn:")
                    .AddChoices(new[] { REASON_OUT_OF_STOCK, REASON_OTHER, "Quay lại" })
            );

            if (reasonChoice == "Quay lại") return null;

            if (reasonChoice == REASON_OTHER)
            {
                return AnsiConsole.Ask<string>("Nhập [green]lý do hủy chi tiết[/]:");
            }
            return reasonChoice;
        }

        




        #endregion // huỷ đơn hàng phía admin

        private async Task ProcessOrdersIndividuallyAndReport(List<Order> orders, int adminId)
        {
            var succeededOrders = new List<Order>();
            var failedOrders = new List<(Order Order, string Reason)>();

            await AnsiConsole.Status()
                .Spinner(Spinner.Known.Dots)
                .StartAsync($"Đang xử lý {orders.Count} đơn hàng...", async ctx =>
                {
                    foreach (var order in orders)
                    {
                        var response = await _orderService.AttemptToConfirmOrderAsync(order.OrderID, adminId);
                        if (response.IsSuccess)
                        {
                            succeededOrders.Add(order);
                        }
                        else
                        {
                            failedOrders.Add((order, response.Message));
                        }
                    }
                });

            DisplayConfirmationReport(succeededOrders, failedOrders);
        }

        private void DisplayConfirmationReport(List<Order> succeededOrders, List<(Order Order, string Reason)> failedOrders)
        {
            AnsiConsole.Clear();
            AnsiConsole.Write(new Rule("[bold yellow]BÁO CÁO KẾT QUẢ XỬ LÝ[/]").Centered());

            if (succeededOrders.Any())
            {
                AnsiConsole.MarkupLine($"\n[green]Đã xác nhận thành công {succeededOrders.Count} đơn hàng:[/]");
                var successTable = new Table().Border(TableBorder.Rounded).AddColumn("ID").AddColumn("Mã Đơn");
                foreach (var order in succeededOrders)
                {
                    successTable.AddRow(order.OrderID.ToString(), Markup.Escape(order.OrderCode));
                }
                AnsiConsole.Write(successTable);
            }

            if (failedOrders.Any())
            {
                AnsiConsole.MarkupLine($"\n[red]Không thể xác nhận {failedOrders.Count} đơn hàng:[/]");
                var failureTable = new Table().Border(TableBorder.Rounded).AddColumn("ID").AddColumn("Mã Đơn").AddColumn("Lý do thất bại");
                foreach (var (order, reason) in failedOrders)
                {
                    failureTable.AddRow(order.OrderID.ToString(), Markup.Escape(order.OrderCode), Markup.Escape(reason));
                }
                AnsiConsole.Write(failureTable);
            }
        }


        
        private List<Order> ProcessSelection(List<Order> selectedItems, List<Order> currentBatch)
        {
            bool selectAllWasChosen = selectedItems.Any(o => o.OrderID == -1);

            if (selectAllWasChosen)
            {
                return currentBatch;
            }

            return selectedItems.Where(o => o.OrderID != -1).ToList();
        }

        private MultiSelectionPrompt<Order> CreateMultiSelectPrompt(List<Order> batch, string actionType)
        {
            var selectAllOrder = new Order { OrderID = -1, OrderCode = SELECT_ALL_CHOICE };
            var selectionList = new List<Order> { selectAllOrder }.Concat(batch).ToList();

            return new MultiSelectionPrompt<Order>()
                .Title($"\n[bold]Chọn các đơn hàng cần {actionType} từ danh sách chi tiết ở trên:[/]")
                .PageSize(12)
                .MoreChoicesText("[grey](Điều hướng bằng phím lên/xuống)[/]")
                .InstructionsText("[grey](Dùng [blue]phím cách[/] để chọn, [green]enter[/] để xác nhận)[/]")
                .UseConverter(order => {
                    if (order.OrderID == -1) return order.OrderCode;
                    return $"[bold]ID: {order.OrderID}[/] - Khách hàng: {Markup.Escape(order.User.FullName)}";
                })
                .AddChoices(selectionList);
        }






        private Table CreateDetailedOrderTable(List<Order> batch)
        {
            var table = new Table().Expand().Border(TableBorder.Rounded);
            table.Title = new TableTitle($"[yellow]Chi tiết {batch.Count} Danh sách đơn hàng[/]");
            table.AddColumn("ID");
            table.AddColumn("Mã Đơn");
            table.AddColumn("Khách hàng");
            table.AddColumn("Thời gian");
            table.AddColumn("Tổng Giá");
            table.AddColumn("Trạng thái");

            foreach (var order in batch)
            {
                table.AddRow(
            new Markup(Markup.Escape(order.OrderID.ToString())),
            new Markup(Markup.Escape(order.OrderCode)),
            new Markup(Markup.Escape(order.User.FullName)),
            new Markup(Markup.Escape(order.OrderDate.ToString("HH:mm dd/MM/yy"))),
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
            AnsiConsole.MarkupLine("\n[dim]Nhấn phím bất kỳ để quay lại danh sách...[/]");
            Console.ReadKey();
        }

        private void DisplayOrderDetails(Order order)
        {
   
            var infoGrid = new Grid()
                .AddColumn().AddColumn()
                .AddRow(new Markup("[bold]Mã đơn:[/]"), new Markup(Markup.Escape(order.OrderCode)))
                .AddRow(new Markup("[bold]Khách hàng:[/]"), new Markup(Markup.Escape(order.User?.FullName ?? "N/A")))
                .AddRow(new Markup("[bold]Ngày đặt:[/]"), new Markup(Markup.Escape(order.OrderDate.ToString("g"))))
                .AddRow(new Markup("[bold]Trạng thái:[/]"), FormatOrderStatus(order.Status))
                .AddRow(new Markup("[bold]Địa chỉ giao:[/]"), new Markup(Markup.Escape(order.ShippingAddress)))
                .AddRow(new Markup("[bold]SĐT Nhận:[/]"), new Markup(Markup.Escape(order.ShippingPhone)))
     
                .AddRow(new Markup("[bold]Thanh toán:[/]"), new Markup(Markup.Escape(order.PaymentMethod ?? "N/A")));

     
            var infoPanel = new Panel(infoGrid)
                .Header($"CHI TIẾT ĐƠN HÀNG - ID: {order.OrderID}")
                .Expand();

            
            var productTable = new Table().Expand().Border(TableBorder.Rounded);
            productTable.Title = new TableTitle("Sản phẩm trong đơn");
            productTable.AddColumn("Sản phẩm");
            productTable.AddColumn("Size");
            productTable.AddColumn("SL");
            productTable.AddColumn("Đơn giá");
            productTable.AddColumn("Thành tiền");

            if (order.OrderDetails != null)
            {
                foreach (var detail in order.OrderDetails)
                {
                    productTable.AddRow(
                        new Markup(Markup.Escape(detail.Product.Name)),
                        new Markup(Markup.Escape(detail.Size.ToString())),
                        new Markup(Markup.Escape(detail.Quantity.ToString())),
                        new Markup($"{detail.UnitPrice:N0} VNĐ"),
                        new Markup($"[bold]{(detail.UnitPrice * detail.Quantity):N0} VNĐ[/]")
                    );
                }
            }

            
            AnsiConsole.Clear();
            AnsiConsole.Write(infoPanel);
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
    }
}
