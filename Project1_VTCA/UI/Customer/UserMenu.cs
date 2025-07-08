using Microsoft.Extensions.DependencyInjection;
using Project1_VTCA.Services.Interface;
using Project1_VTCA.UI.Customer.Interfaces;
using Project1_VTCA.UI.Draw;
using Spectre.Console;
using System.Threading.Tasks;

namespace Project1_VTCA.UI.Customer
{
    public class UserMenu : IUserMenu
    {
        private readonly ISessionService _sessionService;
        private readonly IAccountManagementMenu _accountMenu;
        private readonly IServiceProvider _serviceProvider;

        public UserMenu(ISessionService sessionService, IAccountManagementMenu accountMenu, IServiceProvider serviceProvider)
        {
            _sessionService = sessionService;
            _accountMenu = accountMenu;
            _serviceProvider = serviceProvider;
        }

        public async Task Show()
        {
            while (true)
            {
                AnsiConsole.Clear();
                Banner.Show();
                AnsiConsole.MarkupLine($"Chào mừng trở lại, [bold yellow]{_sessionService.CurrentUser.FullName}[/]!");
                AnsiConsole.WriteLine();

                var choice = AnsiConsole.Prompt(
                    new SelectionPrompt<string>()
                        .Title("\n[bold underline]MENU KHÁCH HÀNG[/]")
                        .PageSize(10)
                        .AddChoices(new[]
                        {
                            "Duyệt sản phẩm (Tất cả sản phẩm)",
                            "Xem giỏ hàng",
                            "Quản lý tài khoản", 
                            "[red]Đăng xuất[/]"
                        }));

                switch (choice)
                {
                    case "Duyệt sản phẩm (Tất cả sản phẩm)":
                        var productMenu = _serviceProvider.GetRequiredService<ProductMenu>();
                        await productMenu.ShowAllProductsAsync();
                        break;
                    case "Xem giỏ hàng":
                        var cartMenu = _serviceProvider.GetRequiredService<ICartMenu>();
                        await cartMenu.ShowAsync();
                        break;
                    case "Quản lý tài khoản":
                        await _accountMenu.ShowAsync(); // GỌI MENU CON
                        break;
                    case "[red]Đăng xuất[/]":
                        _sessionService.LogoutUser();
                        AnsiConsole.MarkupLine("[green]Bạn đã đăng xuất thành công.[/]");
                        Console.ReadKey();
                        return;
                }
            }
        }
    }
}