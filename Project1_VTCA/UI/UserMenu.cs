using Project1_VTCA.Services.Interface;
using Project1_VTCA.UI.Draw;
using Project1_VTCA.UI.Interface;
using Spectre.Console;
using System.Threading.Tasks;

namespace Project1_VTCA.UI
{
    public class UserMenu : IUserMenu
    {
        private readonly ProductMenu _productMenu;
        private readonly ICartMenu _cartMenu;
        private readonly ISessionService _sessionService;
        private readonly IAccountManagementMenu _accountMenu;

        // Cập nhật constructor để nhận IAccountManagementMenu
        public UserMenu(ProductMenu productMenu, ISessionService sessionService, ICartMenu cartMenu, IAccountManagementMenu accountMenu)
        {
            _productMenu = productMenu;
            _cartMenu = cartMenu;
            _sessionService = sessionService;
            _accountMenu = accountMenu; // Gán đối tượng menu con
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
                        await _productMenu.ShowAllProductsAsync();
                        break;
                    case "Xem giỏ hàng":
                        await _cartMenu.ShowAsync();
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