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
        private readonly IAuthService _authService;
        private readonly ISessionService _sessionService;
        private readonly ProductMenu _productMenu;
        private readonly IUserMenu _userMenu;
        private readonly IAdminMenu _adminMenu;

        public MainMenu(IAuthService authService, ISessionService sessionService, ProductMenu productMenu, IUserMenu userMenu, IAdminMenu adminMenu)
        {
            _authService = authService;
            _sessionService = sessionService;
            _productMenu = productMenu;
            _userMenu = userMenu;
            _adminMenu = adminMenu;
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
                    // Sau khi đăng nhập, các menu con sẽ được gọi
                    if (_sessionService.CurrentUser.Role == "Admin") await _adminMenu.Show();
                    else await _userMenu.Show();
                }
            }
        }

        private async Task ShowGuestMenu()
        {
            AnsiConsole.Clear();
            Banner.Show();
            AnsiConsole.Write(new Panel("[italic]Nơi bạn tìm thấy những đôi giày thể hiện cá tính.[/]").Header("Về chúng tôi").Border(BoxBorder.None).Expand());
            AnsiConsole.WriteLine();

            var choice = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("[bold underline]BẮT ĐẦU TRẢI NGHIỆM[/]")
                    .AddChoices(new[] { "Xem danh sách sản phẩm", "Đăng nhập", "Đăng ký", "[red]Thoát chương trình[/]" }));

            switch (choice)
            {
                case "Xem danh sách sản phẩm":
                    await _productMenu.ShowAllProductsAsync();
                    break;
                case "Đăng nhập":
                    await HandleLogin();
                    break;
                case "Đăng ký":
                    await HandleRegistration();
                    break;
                case "[red]Thoát chương trình[/]":
                    AnsiConsole.MarkupLine("[bold green]Cảm ơn bạn đã sử dụng dịch vụ. Tạm biệt![/]");
                    Environment.Exit(0);
                    break;
            }
        }

        // Tầng UI chịu trách nhiệm thu thập dữ liệu và gọi Service
        private async Task HandleLogin()
        {
            while (true)
            {
                AnsiConsole.Clear();
                Banner.Show();
                AnsiConsole.Write(new Rule("[bold yellow]ĐĂNG NHẬP[/]").Centered());

                var username = AnsiConsole.Ask<string>("Nhập [green]Username[/]:");
                var password = AnsiConsole.Prompt(new TextPrompt<string>("Nhập [green]Password[/]:").Secret());

                var result = await _authService.LoginAsync(username, password);
                AnsiConsole.MarkupLine($"\n{result.Message}");

                if (result.IsSuccess)
                {
                    AnsiConsole.Markup("\n[dim]Nhấn phím bất kỳ để vào trang chính...[/]");
                    Console.ReadKey();
                    return;
                }

                var choice = AnsiConsole.Prompt(
                    new SelectionPrompt<string>()
                        .Title("Bạn muốn làm gì tiếp theo?")
                        .AddChoices(new[] { "Thử lại", "Quên mật khẩu", "Quay về Menu chính" }));

                if (choice == "Quên mật khẩu")
                {
                    await HandleForgotPassword();
                    return;
                }
                if (choice == "Quay về Menu chính") return;
            }
        }

        // Tầng UI chịu trách nhiệm thu thập dữ liệu và gọi Service
        private async Task HandleRegistration()
        {
            AnsiConsole.Clear();
            Banner.Show();
            AnsiConsole.Write(new Rule("[bold yellow]ĐĂNG KÝ TÀI KHOẢN MỚI[/]").Centered());

            var dto = new UserRegistrationDto();
            dto.Username = ConsoleHelper.PromptForInput("Nhập [green]Username[/] (viết liền, không dấu, 3-20 ký tự):", InputValidator.IsValidUsername, "Username không hợp lệ.");
            if (dto.Username == null) { AnsiConsole.MarkupLine("[yellow]Đã hủy đăng ký.[/]"); Console.ReadKey(); return; }

            dto.Password = ConsoleHelper.PromptForInput("Nhập [green]Password[/] (8-20 ký tự, có hoa, thường, số, ký tự đặc biệt):", InputValidator.IsValidPassword, "Mật khẩu không đủ mạnh.");
            if (dto.Password == null) { AnsiConsole.MarkupLine("[yellow]Đã hủy đăng ký.[/]"); Console.ReadKey(); return; }

            dto.FullName = ConsoleHelper.PromptForInput("Nhập [green]Họ và Tên[/]:", f => !string.IsNullOrWhiteSpace(f), "Họ và tên không được để trống.");
            if (dto.FullName == null) { AnsiConsole.MarkupLine("[yellow]Đã hủy đăng ký.[/]"); Console.ReadKey(); return; }

            dto.Email = ConsoleHelper.PromptForInput("Nhập [green]Email[/]:", InputValidator.IsValidEmail, "Email không hợp lệ.");
            if (dto.Email == null) { AnsiConsole.MarkupLine("[yellow]Đã hủy đăng ký.[/]"); Console.ReadKey(); return; }

            dto.PhoneNumber = ConsoleHelper.PromptForInput("Nhập [green]Số điện thoại[/]:", InputValidator.IsValidPhoneNumber, "Số điện thoại không hợp lệ (cần đủ 10 số).");
            if (dto.PhoneNumber == null) { AnsiConsole.MarkupLine("[yellow]Đã hủy đăng ký.[/]"); Console.ReadKey(); return; }

            dto.Gender = AnsiConsole.Prompt(new SelectionPrompt<string>().Title("Chọn [green]Giới tính[/]:").AddChoices(new[] { "Male", "Female", "Unisex" }));

            var result = await _authService.RegisterAsync(dto);
            AnsiConsole.MarkupLine($"\n{result.Message}");
            AnsiConsole.Markup("\n[dim]Nhấn phím bất kỳ để tiếp tục...[/]");
            Console.ReadKey();
        }

        private async Task HandleForgotPassword()
        {
            AnsiConsole.Clear();
            Banner.Show();
            AnsiConsole.Write(new Rule("[bold yellow]QUÊN MẬT KHẨU[/]").Centered());
            // AuthService sẽ tự hỏi các thông tin cần thiết
            var result = await _authService.ForgotPasswordAsync();
            AnsiConsole.MarkupLine($"\n{result.Message}");
            AnsiConsole.Markup("\n[dim]Nhấn phím bất kỳ để tiếp tục...[/]");
            Console.ReadKey();
        }
    }
}