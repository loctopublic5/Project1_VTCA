using Project1_VTCA.DTOs;
using Project1_VTCA.Services;
using Project1_VTCA.Utils;
using Spectre.Console;
using System;
using System.Threading.Tasks;

namespace Project1_VTCA.UI
{
    public class MainMenu
    {
        private readonly ProductMenu _productMenu;
        private readonly IAuthService _authService;
        private readonly ISessionService _sessionService;
        private readonly IUserMenu _userMenu;
        private readonly IAdminMenu _adminMenu;
        private readonly ConsoleLayout _layout;

        public MainMenu(ProductMenu productMenu, IAuthService authService, ISessionService sessionService, IUserMenu userMenu, IAdminMenu adminMenu, ConsoleLayout layout)
        {
            _productMenu = productMenu;
            _authService = authService;
            _sessionService = sessionService;
            _userMenu = userMenu;
            _adminMenu = adminMenu;
            _layout = layout;
        }

        public async Task Show()
        {
            while (true)
            {
                if (!_sessionService.IsLoggedIn)
                {
                    await ShowGuestMenu();
                }
                else
                {
                    if (_sessionService.CurrentUser.Role == "Admin")
                    {
                        await _adminMenu.Show();
                    }
                    else
                    {
                        await _userMenu.Show();
                    }
                }
            }
        }

        private async Task ShowGuestMenu()
        {
            AnsiConsole.Clear();
            Banner.Show();

            AnsiConsole.Write(
                new Panel("[italic]Nơi bạn tìm thấy những đôi giày thể hiện cá tính. " +
                          "Chúng tôi cam kết chất lượng và phong cách trong từng sản phẩm.[/]")
                    .Header("Về chúng tôi")
                    .Border(BoxBorder.None)
                    .Expand());
            AnsiConsole.WriteLine();

            var choice = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("[bold underline]BẮT ĐẦU TRẢI NGHIỆM[/]")
                    .PageSize(5)
                    .AddChoices(new[] { "Xem danh sách sản phẩm", "Đăng nhập", "Đăng ký", "[red]Thoát chương trình[/]" }));

            switch (choice)
            {
                case "Xem danh sách sản phẩm":
                    await _productMenu.ShowAllProductsAsync();
                    break;
                case "Đăng nhập":
                    AnsiConsole.Clear();
                    Banner.Show();
                    AuthResult loginResult = await _authService.LoginAsync();
                    AnsiConsole.MarkupLine(loginResult.Message);
                    AnsiConsole.Markup("\n[dim]Nhấn phím bất kỳ để tiếp tục...[/]");
                    Console.ReadKey();
                    break;
                case "Đăng ký":
                    AnsiConsole.Clear();
                    Banner.Show();
                    AuthResult registerResult = await _authService.RegisterAsync();
                    AnsiConsole.MarkupLine(registerResult.Message);
                    AnsiConsole.Markup("\n[dim]Nhấn phím bất kỳ để tiếp tục...[/]");
                    Console.ReadKey();
                    break;
                case "[red]Thoát chương trình[/]":
                    AnsiConsole.MarkupLine("[bold green]Cảm ơn bạn đã sử dụng dịch vụ. Tạm biệt![/]");
                    Environment.Exit(0);
                    break;
            }
        }
    }
}