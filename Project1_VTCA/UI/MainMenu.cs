using Project1_VTCA.DTOs;
using Project1_VTCA.Services.Interface;
using Project1_VTCA.UI.Customer;
using Project1_VTCA.UI.Customer.Interface;
using Project1_VTCA.UI.Draw;
using Project1_VTCA.Utils;
using Spectre.Console;
using System;
using System.Collections.Generic;
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
                    if (_sessionService.CurrentUser.Role == "Admin") await _adminMenu.Show();
                    else await _userMenu.Show();
                }
            }
        }

        private async Task ShowGuestMenu()
        {
            var options = new List<string> { "Xem sản phẩm", "Đăng nhập", "Đăng ký", "Thoát" };
            var choice = MenuHelper.ShowHorizontalMenu("BẮT ĐẦU TRẢI NGHIỆM", options);

            switch (choice)
            {
                case "Xem sản phẩm":
                    await _productMenu.ShowAllProductsAsync();
                    break;
                case "Đăng nhập":
                    await HandleLogin();
                    break;
                case "Đăng ký":
                    await HandleRegistration();
                    break;
                case "Thoát":
                    AnsiConsole.MarkupLine("[bold green]Cảm ơn bạn đã sử dụng dịch vụ. Tạm biệt![/]");
                    Environment.Exit(0);
                    break;
            }
        }

        private async Task HandleLogin()
        {
            while (true)
            {
                AnsiConsole.Clear();
                Banner.Show();
                AnsiConsole.Write(new Rule("[bold yellow]ĐĂNG NHẬP[/]").Centered());

                var username = AnsiConsole.Ask<string>("Nhập [green]Username[/] (hoặc 'exit' để thoát):");
                if (username.Equals("exit", StringComparison.OrdinalIgnoreCase))
                {
                    return; 
                }

                var password = AnsiConsole.Prompt(new TextPrompt<string>("Nhập [green]Password[/]:").Secret()).Trim();

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
                }
                else if (choice == "Quay về Menu chính")
                {
                    return;
                }
            }
        }

        private async Task HandleRegistration()
        {
            AnsiConsole.Clear();
            Banner.Show();
            AnsiConsole.Write(new Rule("[bold yellow]ĐĂNG KÝ TÀI KHOẢN MỚI[/]").Centered());
            AnsiConsole.MarkupLine("[dim](Nhập 'exit' bất cứ lúc nào để hủy bỏ)[/]\n");

            var dto = new UserRegistrationDto();

            dto.Username = ConsoleHelper.PromptForInput("Nhập [green]Username[/] (3-20 ký tự, không dấu, không khoảng trắng):", InputValidator.IsValidUsername, "Username không hợp lệ.");
            if (dto.Username == null) { AnsiConsole.MarkupLine("[yellow]Đã hủy đăng ký.[/]"); Console.ReadKey(); return; }

            dto.Password = AnsiConsole.Prompt(
                new TextPrompt<string>("Nhập [green]Password[/] (8-20 ký tự, có hoa, thường, số, ký tự đặc biệt):")
                    .Secret()
                    .Validate(password =>
                        InputValidator.IsValidPassword(password)
                        ? ValidationResult.Success()
                        : ValidationResult.Error("[red]Mật khẩu không đủ mạnh.[/]"))
            );

            dto.FullName = ConsoleHelper.PromptForInput("Nhập [green]Họ và Tên[/]:", f => !string.IsNullOrWhiteSpace(f), "Họ và tên không được để trống.");
            if (dto.FullName == null) { AnsiConsole.MarkupLine("[yellow]Đã hủy đăng ký.[/]"); Console.ReadKey(); return; }

            // Cập nhật lại text hướng dẫn cho email
            dto.Email = ConsoleHelper.PromptForInput("Nhập [green]Email[/] (phải là @gmail.com, chữ thường):", InputValidator.IsValidEmail, "Email không hợp lệ.");
            if (dto.Email == null) { AnsiConsole.MarkupLine("[yellow]Đã hủy đăng ký.[/]"); Console.ReadKey(); return; }

            dto.PhoneNumber = ConsoleHelper.PromptForInput("Nhập [green]Số điện thoại[/]:", InputValidator.IsValidPhoneNumber, "Số điện thoại không hợp lệ (phải bắt đầu bằng 0, đủ 10 số).");
            if (dto.PhoneNumber == null) { AnsiConsole.MarkupLine("[yellow]Đã hủy đăng ký.[/]"); Console.ReadKey(); return; }

            // CẬP NHẬT LỰA CHỌN GIỚI TÍNH
            dto.Gender = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("Chọn [green]Giới tính[/]:")
                    .AddChoices(new[] { "Male", "Female" }) // Chỉ còn 2 lựa chọn
            );

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
            AnsiConsole.MarkupLine("[dim](Nhập 'exit' bất cứ lúc nào để hủy bỏ)[/]\n");

            string username = ConsoleHelper.PromptForInput("Nhập [green]Username[/] của bạn:", u => !string.IsNullOrWhiteSpace(u), "Username không được để trống.");
            if (username == null) { AnsiConsole.MarkupLine("[yellow]Đã hủy thao tác.[/]"); Console.ReadKey(); return; }

            string email = ConsoleHelper.PromptForInput("Nhập [green]Email[/] đã đăng ký để xác thực:", InputValidator.IsValidEmail, "Email không hợp lệ.");
            if (email == null) { AnsiConsole.MarkupLine("[yellow]Đã hủy thao tác.[/]"); Console.ReadKey(); return; }

            string newPassword = ConsoleHelper.PromptForInput("Nhập [green]Mật khẩu mới[/] (8-20 ký tự, có hoa, thường, số, ký tự đặc biệt):", InputValidator.IsValidPassword, "Mật khẩu không đủ mạnh.");
            if (newPassword == null) { AnsiConsole.MarkupLine("[yellow]Đã hủy thao tác.[/]"); Console.ReadKey(); return; }

            var result = await _authService.ForgotPasswordAsync(username, email, newPassword);
            AnsiConsole.MarkupLine($"\n{result.Message}");
            AnsiConsole.Markup("\n[dim]Nhấn phím bất kỳ để quay lại...[/]");
            Console.ReadKey();
        }
    }
}