using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Spectre.Console;
using System;
using System.Threading.Tasks;
using Project1_VTCA.Services;

namespace Project1_VTCA.UI
{
    public class MainMenu
    {
        private readonly ConsoleLayout _layout;
        // private readonly IProductService _productService; // Sẽ dùng sau
        // private readonly IAuthService _authService; // Sẽ dùng sau

        public MainMenu(ConsoleLayout layout/*, IProductService productService, IAuthService authService*/)
        {
            _layout = layout;
            // _productService = productService;
            // _authService = authService;
        }

        public async Task Show()
        {
            string notification = "Chào mừng đến với Cửa hàng Giày!";

            while (true)
            {
                var menu = new Markup(
                    "[bold]1. Xem danh sách sản phẩm[/]\n" +
                    "[bold]2. Đăng nhập[/]\n" +
                    "[bold]3. Đăng ký[/]\n" +
                    "[bold]4. [red]Thoát[/][/]"
                );

                var view = new Text("Sử dụng các phím số để điều hướng menu.");

                _layout.Render(menu, view, new Markup($"[yellow]{notification}[/]"));

                Console.Write("\n> Nhập lựa chọn của bạn: ");
                string choice = Console.ReadLine();

                switch (choice)
                {
                    case "1":
                        notification = "Chức năng 'Xem sản phẩm' sẽ được làm ở bước 2.1";
                        break;
                    case "2":
                        notification = "Chức năng 'Đăng nhập' sẽ được làm ở bước 2.2";
                        break;
                    case "3":
                        notification = "Chức năng 'Đăng ký' sẽ được làm ở bước 2.2";
                        break;
                    case "4":
                        AnsiConsole.MarkupLine("[bold green]Cảm ơn bạn đã sử dụng dịch vụ. Tạm biệt![/]");
                        return;
                    default:
                        notification = "[red]Lựa chọn không hợp lệ. Vui lòng chọn lại.[/]";
                        break;
                }
            }
        }
    }
}
