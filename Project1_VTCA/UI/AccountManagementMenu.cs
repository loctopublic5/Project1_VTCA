using Project1_VTCA.UI.Interface;
using Spectre.Console;
using System.Threading.Tasks;

namespace Project1_VTCA.UI
{
    public class AccountManagementMenu : IAccountManagementMenu
    {
        private readonly IAddressMenu _addressMenu;
        private readonly IWalletMenu _walletMenu;

        public AccountManagementMenu(IAddressMenu addressMenu, IWalletMenu walletMenu)
        {
            _addressMenu = addressMenu;
            _walletMenu = walletMenu;
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
                            "Nạp tiền vào tài khoản",
                            "Xem lịch sử giao dịch (sắp có)",
                            "[red]Quay lại Menu chính[/]"
                        })
                );

                switch (choice)
                {
                    case "Quản lý địa chỉ":
                        await _addressMenu.ShowAddressManagementAsync();
                        break;
                    case "Nạp tiền vào tài khoản":
                        await _walletMenu.ShowWalletAsync();
                        break;
                    case "Xem lịch sử giao dịch (sắp có)":
                        AnsiConsole.MarkupLine("[yellow]Chức năng đang được xây dựng.[/]");
                        Console.ReadKey();
                        break;
                    case "[red]Quay lại Menu chính[/]":
                        return;
                }
            }
        }
    }
}