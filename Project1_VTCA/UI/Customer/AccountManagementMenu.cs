using Project1_VTCA.Services.Interface;
using Project1_VTCA.UI.Customer.Interfaces;
using Spectre.Console;
using System.Threading.Tasks;

namespace Project1_VTCA.UI.Customer
{
    public class AccountManagementMenu : IAccountManagementMenu
    {
        private readonly IAddressMenu _addressMenu;
        private readonly IMyWalletMenu _myWalletMenu;
        private readonly IOrderHistoryMenu _orderHistoryMenu;
        private readonly ISessionService _sessionService;

        public AccountManagementMenu(IAddressMenu addressMenu, IMyWalletMenu myWalletMenu, IOrderHistoryMenu orderHistoryMenu, ISessionService sessionService)
        {
            _addressMenu = addressMenu;
            _myWalletMenu = myWalletMenu;
            _orderHistoryMenu = orderHistoryMenu;
            _sessionService = sessionService;
        }

        public async Task ShowAsync()
        {
            while (true)
            {
                AnsiConsole.Clear();
                AnsiConsole.Write(new Rule("[bold yellow]QUẢN LÝ TÀI KHOẢN[/]"));

                AnsiConsole.MarkupLine($"[bold cyan]Tổng chi tiêu của bạn đến nay: {_sessionService.CurrentUser.TotalSpending:N0} VNĐ[/]");
                AnsiConsole.WriteLine();

                var choice = AnsiConsole.Prompt(
                    new SelectionPrompt<string>()
                        .Title("[bold]Mời bạn chọn một chức năng:[/]")
                        .AddChoices(new[] {
                            "Quản lý địa chỉ",
                            "Ví của tôi",
                            "Quản lý đơn hàng",
                            "[red]Quay lại Menu chính[/]"
                        })
                );

                switch (choice)
                {
                    case "Quản lý địa chỉ":
                        await _addressMenu.ShowAddressManagementAsync();
                        break;
                    case "Ví của tôi":
                        await _myWalletMenu.ShowWalletAsync();
                        break;
                    case "Quản lý đơn hàng":
                        await _orderHistoryMenu.ShowAsync();
                        break;
                    case "[red]Quay lại Menu chính[/]":
                        return;
                }
            }
        }
    }
}