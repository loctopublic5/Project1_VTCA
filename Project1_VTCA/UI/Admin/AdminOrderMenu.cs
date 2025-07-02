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
        // Hằng số cho lựa chọn đặc biệt, giúp code dễ đọc và bảo trì
        private const string SELECT_ALL_CHOICE = "[bold yellow](Chọn/Bỏ chọn Tất cả các đơn trong lô này)[/]";

        public AdminOrderMenu(IOrderService orderService, ISessionService sessionService, ConsoleLayout layout)
        {
            _orderService = orderService;
            _sessionService = sessionService;
            _layout = layout;
        }

        public async Task ShowAsync()
        {
            while (true)
            {
                var menuContent = CreateMainMenu();
                var viewContent = new FigletText("Admin Center").Centered().Color(Color.Yellow);
                var notificationContent = new Markup("[dim]Chọn một hành động từ menu bên trái.[/]");

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
                        await HandleBulkApproveCancellationsAsync();
                        Console.ReadKey();
                        break;
                    case "4":
                        AnsiConsole.MarkupLine("[yellow]Chức năng đang được xây dựng.[/]");
                        Console.ReadKey();
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
                " 2. Hủy Đơn hàng (Từ chối)\n" +
                " 3. Duyệt Yêu cầu Hủy\n\n" +
                "[bold]Tra cứu:[/]\n" +
                " 4. Lọc và Xem đơn hàng\n\n" +
                " [red]0. Quay lại[/]"
            );
        }
        #region xác nhận đơn hàng
        private async Task HandleBulkConfirmFlowAsync()
        {
            const int batchSize = 10;
            while (true)
            {
                var (batch, _) = await _orderService.GetOrdersForAdminAsync("PendingAdminApproval", 1, batchSize);
                if (!batch.Any())
                {
                    AnsiConsole.MarkupLine("\n[green]Tuyệt vời! Đã xử lý xong tất cả các đơn hàng đang chờ xác nhận.[/]");
                    Console.ReadKey();
                    break;
                }

                var mainViewPrompt = CreateMultiSelectPrompt(batch);
                var sideViewContent = CreateDetailedOrderTable(batch);

                _layout.Render(new Markup(""), sideViewContent, new Markup(mainViewPrompt.Title ?? ""));


                var selectedOrders = AnsiConsole.Prompt(mainViewPrompt);
                var finalSelection = ProcessSelection(selectedOrders, batch);

                if (!finalSelection.Any())
                {
                    if (AnsiConsole.Confirm("[yellow]Bạn không chọn đơn hàng nào. Bạn có muốn thoát khỏi chức năng này không?[/]")) break;
                    continue;
                }

                // --- BẮT ĐẦU LUỒNG XỬ LÝ TỪNG PHẦN ---
                var succeededOrders = new List<Order>();
                var failedOrders = new List<(Order Order, string Reason)>();
                var adminId = _sessionService.CurrentUser.UserID;

                await AnsiConsole.Status()
                    .Spinner(Spinner.Known.Dots)
                    .StartAsync($"Đang xử lý {finalSelection.Count} đơn hàng...", async ctx =>
                    {
                        foreach (var order in finalSelection)
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

                // --- HIỂN THỊ BÁO CÁO KẾT QUẢ ---
                DisplayConfirmationReport(succeededOrders, failedOrders);
                AnsiConsole.MarkupLine("\n[dim]Nhấn phím bất kỳ để tiếp tục...[/]");
                Console.ReadKey();

                if (AnsiConsole.Confirm("Bạn có muốn tiếp tục xử lý các lô đơn hàng khác không?")) continue;
                break;
            }
        }
        #endregion

        #region xác nhận đơn huỷ từ khách hàng
        private async Task HandleBulkApproveCancellationsAsync()
        {
            // 1. Lấy tất cả đơn hàng đang chờ duyệt hủy
            var (ordersToApprove, _) = await _orderService.GetOrdersForAdminAsync("CancellationRequested", 1, 999);

            if (!ordersToApprove.Any())
            {
                AnsiConsole.MarkupLine("\n[green]Không có yêu cầu hủy nào từ khách hàng.[/]");
                Console.ReadKey();
                return;
            }

            // 2. Hiển thị giao diện 3 khung
            var menuContent = new Markup($"[bold yellow]DUYỆT YÊU CẦU HỦY[/]\n\n[dim]Tìm thấy [yellow]{ordersToApprove.Count}[/] yêu cầu cần duyệt.[/]");
            var viewContent = CreateCancellationApprovalTable(ordersToApprove);
            var notificationContent = new Markup("[dim]Xem lại danh sách và xác nhận hành động bên dưới.[/]");

            _layout.Render(menuContent, viewContent, notificationContent);

            // 3. Hành động xác nhận của Admin
            if (AnsiConsole.Confirm($"\nBạn có chắc chắn muốn [green]chấp thuận hủy toàn bộ {ordersToApprove.Count} đơn hàng[/] được liệt kê ở trên không?"))
            {
                var orderIds = ordersToApprove.Select(o => o.OrderID).ToList();
                var adminId = _sessionService.CurrentUser.UserID;

                // 4. Gọi service để xử lý hàng loạt
                var response = await _orderService.BulkApproveCancellationsAsync(orderIds, adminId);

                AnsiConsole.MarkupLine($"\n[{(response.IsSuccess ? "green" : "red")}]{Markup.Escape(response.Message)}[/]");
                Console.ReadKey();
            }
            else
            {
                AnsiConsole.MarkupLine("[yellow]Đã hủy thao tác.[/]");
                Console.ReadKey();
            }
        }

        private Table CreateCancellationApprovalTable(List<Order> orders)
        {
            var table = new Table().Expand().Border(TableBorder.Rounded);
            table.Title = new TableTitle("[bold]Danh sách đơn hàng đang chờ duyệt hủy[/]");
            table.AddColumn("ID Đơn hàng");
            table.AddColumn("Khách hàng");
            table.AddColumn("Tổng tiền");
            // Cột quan trọng nhất
            table.AddColumn(new TableColumn("[orange1]Lý do hủy của khách[/]").NoWrap());

            foreach (var order in orders)
            {
                table.AddRow(
                    Markup.Escape(order.OrderID.ToString()),
                    Markup.Escape(order.User.FullName),
                    $"[yellow]{order.TotalPrice:N0} VNĐ[/]",
                    $"[italic]{Markup.Escape(order.CustomerCancellationReason ?? "Không có lý do")}[/]"
                );
            }
            return table;
        }
        #endregion

        #region huỷ đơn hàng phía admin
        private async Task HandleBulkRejectFlowAsync()
        {
            const int batchSize = 10;
            while (true)
            {
                var (batch, _) = await _orderService.GetOrdersForAdminAsync("PendingAdminApproval", 1, batchSize);
                if (!batch.Any())
                {
                    AnsiConsole.MarkupLine("\n[green]Không còn đơn hàng nào đang chờ để hủy.[/]");
                    Console.ReadKey();
                    break;
                }

                // Hiển thị giao diện ban đầu
                var menuContent = new Markup("[bold]HỦY ĐƠN HÀNG (TỪ CHỐI)[/]\n\n1. [orange1]Bắt đầu Hủy đơn hàng[/]\n0. Quay lại");
                var viewContent = CreateDetailedOrderTable(batch);
                var notificationContent = new Markup("[dim]Xem lại danh sách và chọn hành động từ menu.[/]");
                _layout.Render(menuContent, viewContent, notificationContent);

                var actionChoice = AnsiConsole.Ask<string>("\n> Nhập lựa chọn: ");
                if (actionChoice != "1") break;

                // Khởi tạo hành động hủy
                var mainViewPrompt = ShowMultiSelectPromptForRejection(batch); 
                var selectedOrders = AnsiConsole.Prompt(mainViewPrompt).ToList();
                var finalSelection = ProcessSelection(selectedOrders, batch);




                if (!finalSelection.Any())
                {
                    if (AnsiConsole.Confirm("[yellow]Bạn không chọn đơn hàng nào. Bạn có muốn thoát không?[/]")) break;
                    continue;
                }

                var reason = GetRejectionReason();
                if (reason == null) continue; 

                if (AnsiConsole.Confirm($"Bạn có chắc chắn muốn [red]HỦY {finalSelection.Count} đơn hàng[/] với lý do '{Markup.Escape(reason)}'?"))
                {
                    var orderIds = finalSelection.Select(o => o.OrderID).ToList();
                    var adminId = _sessionService.CurrentUser.UserID;
                    var response = await _orderService.BulkRejectOrdersAsync(orderIds, adminId, reason);

                    AnsiConsole.MarkupLine($"\n[{(response.IsSuccess ? "green" : "red")}]{Markup.Escape(response.Message)}[/]");
                    AnsiConsole.MarkupLine("\n[dim]Nhấn phím bất kỳ để tải lô tiếp theo...[/]");
                    Console.ReadKey();
                }
                else
                {
                    AnsiConsole.MarkupLine("[yellow]Đã hủy thao tác.[/]");
                    if (AnsiConsole.Confirm("Bạn có muốn thoát khỏi chức năng này không?")) break;
                }
            }
        }

        private string? GetRejectionReason()
        {
            var reasonChoice = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("\nChọn [green]lý do hủy[/] chung cho các đơn hàng đã chọn:")
                    .AddChoices(new[] { "Sản phẩm hiện đang hết hàng", "Lý do khác (nhập chi tiết)..." })
            );

            if (reasonChoice == "Lý do khác (nhập chi tiết)...")
            {
                return AnsiConsole.Ask<string>("Nhập [green]lý do hủy chi tiết[/]:");
            }
            return reasonChoice;
        }

        private MultiSelectionPrompt<Order> ShowMultiSelectPromptForRejection(List<Order> batch)
        {
            var selectAllOrder = new Order { OrderID = -1, User = new User { FullName = SELECT_ALL_CHOICE } };
            var selectionList = new List<Order> { selectAllOrder }.Concat(batch).ToList();

            return new MultiSelectionPrompt<Order>()
                .Title("\n[bold]Chọn các đơn hàng cần hủy từ danh sách tham khảo bên phải:[/]")
                .PageSize(12)
                .MoreChoicesText("[grey](Điều hướng bằng phím lên/xuống)[/]")
                .InstructionsText("(Dùng [blue]phím cách[/] để chọn, [green]enter[/] để xác nhận)")
                .UseConverter(order => {
                    if (order.OrderID == -1) return order.User.FullName;
                    return $"[bold]ID: {order.OrderID}[/] - Khách hàng: {Markup.Escape(order.User.FullName)}";
                })
                .AddChoices(selectionList);
        }
        #endregion

        private void DisplayConfirmationReport(List<Order> succeededOrders, List<(Order Order, string Reason)> failedOrders)
        {
            if (succeededOrders.Any())
            {
                AnsiConsole.MarkupLine("[green]Các đơn hàng sau đã được xác nhận thành công:[/]");
                var successTable = new Table().Border(TableBorder.Rounded).AddColumn("ID").AddColumn("Mã Đơn");
                foreach (var order in succeededOrders)
                {
                    successTable.AddRow(order.OrderID.ToString(), Markup.Escape(order.OrderCode));
                }
                AnsiConsole.Write(successTable);
            }

            if (failedOrders.Any())
            {
                AnsiConsole.MarkupLine("[red]Các đơn hàng sau không thể xác nhận:[/]");
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

        private MultiSelectionPrompt<Order> CreateMultiSelectPrompt(List<Order> batch)
        {
            var selectAllOrder = new Order { OrderID = -1, OrderCode = SELECT_ALL_CHOICE };
            var selectionList = new List<Order> { selectAllOrder }.Concat(batch).ToList();

            return new MultiSelectionPrompt<Order>()
                .Title("\n[bold]Chọn các đơn hàng cần xác nhận từ danh sách chi tiết ở trên:[/]")
                .PageSize(12)
                .MoreChoicesText("[grey](Điều hướng bằng phím lên/xuống)[/]")
                .InstructionsText("[grey](Dùng [blue]phím cách[/] để chọn, [green]enter[/] để xác nhận)[/]")
                .UseConverter(order => {
                    if (order.OrderID == -1) return order.OrderCode;
                    // Hiển thị thông tin tối giản trong prompt
                    return $"[bold]ID: {order.OrderID}[/] - Khách hàng: {Markup.Escape(order.User.FullName)}";
                })
                .AddChoices(selectionList);
        }

     


        // Phương thức tạo bảng chi tiết để hiển thị ở View Chính
        private Table CreateDetailedOrderTable(List<Order> batch)
        {
            var table = new Table().Expand().Border(TableBorder.Rounded);
            table.Title = new TableTitle($"[yellow]Chi tiết {batch.Count} đơn hàng trong lô cần xử lý[/]");
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

        private async Task HandleFilterAndViewOrdersAsync()
        {
            AnsiConsole.MarkupLine("[yellow]Chức năng Lọc và Xem đang được xây dựng.[/]");
            Console.ReadKey();
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
    }
}