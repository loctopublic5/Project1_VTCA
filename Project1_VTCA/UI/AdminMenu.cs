using Project1_VTCA.Services.Interface;
using Project1_VTCA.UI.Admin.Interface;
using Project1_VTCA.UI.Interface;
using Spectre.Console;
using System.Threading.Tasks;

namespace Project1_VTCA.UI
{
    public class AdminMenu : IAdminMenu
    {
        private readonly IAdminOrderMenu _adminOrderMenu;
        private readonly ISessionService _sessionService;

        public AdminMenu(IAdminOrderMenu adminOrderMenu, ISessionService sessionService)
        {
            _adminOrderMenu = adminOrderMenu;
            _sessionService = sessionService;
        }

        public async Task Show()
        {
            while (true)
            {
                AnsiConsole.Clear();
                AnsiConsole.Write(new Rule("[bold red]ADMIN DASHBOARD[/]").Centered());

                var choice = AnsiConsole.Prompt(
                    new SelectionPrompt<string>()
                    .Title("\n[bold]Chọn một chức năng quản trị:[/]")
                    .AddChoices(new[] {
                        "Quản lý Đơn hàng",
                        "Quản lý Sản phẩm (sắp có)",
                        "Quản lý Khách hàng (sắp có)",
                        "Đăng xuất"
                    })
                );

                switch (choice)
                {
                    case "Quản lý Đơn hàng":
                        await _adminOrderMenu.ShowAsync();
                        break;
                    case "Quản lý Sản phẩm (sắp có)":
                        AnsiConsole.MarkupLine("[yellow]Chức năng đang được xây dựng.[/]");
                        Console.ReadKey();
                        break;
                    case "Quản lý Khách hàng (sắp có)":
                        AnsiConsole.MarkupLine("[yellow]Chức năng đang được xây dựng.[/]");
                        Console.ReadKey();
                        break;
                    case "Đăng xuất":
                        _sessionService.LogoutUser();
                        AnsiConsole.MarkupLine("\n[green]Bạn đã đăng xuất khỏi tài khoản Admin.[/]");
                        Console.ReadKey();
                        return; // Thoát khỏi vòng lặp và quay về MainMenu
                }
            }
        }
    }
}