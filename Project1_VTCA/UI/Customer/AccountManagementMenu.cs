using Project1_VTCA.UI.Customer.Interface;
using Spectre.Console;
using System.Threading.Tasks;

namespace Project1_VTCA.UI.Customer
{
    public class AccountManagementMenu : IAccountManagementMenu
    {
        private readonly IAddressMenu _addressMenu;
        private readonly IMyWalletMenu _myWalletMenu;
        private readonly IOrderHistoryMenu _orderHistoryMenu;

        public AccountManagementMenu(IAddressMenu addressMenu, IMyWalletMenu myWalletMenu, IOrderHistoryMenu orderHistoryMenu)
        {
            _addressMenu = addressMenu;
            _myWalletMenu = myWalletMenu;
            _orderHistoryMenu = orderHistoryMenu; 
        }

        public async Task ShowAsync()
        {
            while (true)
            {
                AnsiConsole.Clear();
                var choice = AnsiConsole.Prompt(
                    new SelectionPrompt<string>()
                        .Title("[bold underline yellow]QUẢN LÝ TÀI KHOẢN[/]")
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