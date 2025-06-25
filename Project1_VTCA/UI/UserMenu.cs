using Project1_VTCA.Services;
using Project1_VTCA.Utils;
using Spectre.Console;
using System.Threading.Tasks;

namespace Project1_VTCA.UI
{
    public class UserMenu : IUserMenu
    {
        private readonly ProductMenu _productMenu;
        private readonly ISessionService _sessionService;
        // Chúng ta sẽ tiêm các service khác vào đây sau
        // private readonly IFeaturedProductMenu _featuredProductMenu; 

        public UserMenu(ProductMenu productMenu, ISessionService sessionService /*, IFeaturedProductMenu featuredProductMenu */)
        {
            _productMenu = productMenu;
            _sessionService = sessionService;
            // _featuredProductMenu = featuredProductMenu;
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
                            "Xem sản phẩm nổi bật",
                            "Xem giỏ hàng",
                            "Quản lý tài khoản (Địa chỉ, Lịch sử mua hàng...)",
                            "[red]Đăng xuất[/]"
                        }));

                switch (choice)
                {
                    case "Duyệt sản phẩm (Tất cả sản phẩm)":
                        // Gọi đến ProductMenu, nơi sẽ hiển thị layout 3 khung
                        await _productMenu.ShowAllProductsAsync();
                        break;
                    case "Xem sản phẩm nổi bật":
                        AnsiConsole.MarkupLine("[yellow]Chức năng 'Sản phẩm nổi bật' đang được xây dựng.[/]");
                        Console.ReadKey();
                        break;
                    case "Xem giỏ hàng":
                        AnsiConsole.MarkupLine("[yellow]Chức năng 'Giỏ hàng' đang được xây dựng.[/]");
                        Console.ReadKey();
                        break;
                    case "Quản lý tài khoản (Địa chỉ, Lịch sử mua hàng...)":
                        AnsiConsole.MarkupLine("[yellow]Chức năng 'Quản lý tài khoản' đang được xây dựng.[/]");
                        Console.ReadKey();
                        break;
                    case "[red]Đăng xuất[/]":
                        _sessionService.LogoutUser();
                        AnsiConsole.MarkupLine("[green]Bạn đã đăng xuất thành công.[/]");
                        Console.ReadKey();
                        return; // Quay về Guest Menu
                }
            }
        }
    }
}