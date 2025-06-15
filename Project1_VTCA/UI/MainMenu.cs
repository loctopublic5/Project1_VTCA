using Spectre.Console;
using System;
using System.Threading.Tasks;

namespace Project1_VTCA.UI
{
    public class MainMenu
    {
        private readonly ProductMenu _productMenu;
        // Các service khác sẽ được tiêm vào đây sau
        // private readonly IAuthService _authService;

        public MainMenu(ProductMenu productMenu /*, IAuthService authService*/)
        {
            _productMenu = productMenu;
            // _authService = authService;
        }

        public async Task Show()
        {
            while (true)
            {
                AnsiConsole.Clear();
                var choice = AnsiConsole.Prompt(
                    new SelectionPrompt<string>()
                        .Title("[bold underline orchid1]CHÀO MỪNG ĐẾN VỚI SNEAKER SHOP[/]")
                        .PageSize(5)
                        .AddChoices(new[]
                        {
                            "Xem danh sách sản phẩm",
                            "Đăng nhập",
                            "Đăng ký",
                            "[red]Thoát[/]"
                        }));

                switch (choice)
                {
                    case "Xem danh sách sản phẩm":
                        await _productMenu.ShowAllProductsAsync();
                        break;
                    case "Đăng nhập":
                        AnsiConsole.MarkupLine("[yellow]Chức năng 'Đăng nhập' đang được phát triển...[/]");
                        Console.ReadKey();
                        break;
                    case "Đăng ký":
                        AnsiConsole.MarkupLine("[yellow]Chức năng 'Đăng ký' đang được phát triển...[/]");
                        Console.ReadKey();
                        break;
                    case "[red]Thoát[/]":
                        AnsiConsole.MarkupLine("[bold green]Cảm ơn bạn đã sử dụng dịch vụ. Tạm biệt![/]");
                        return;
                }
            }
        }
    }
}