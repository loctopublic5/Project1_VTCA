using Microsoft.EntityFrameworkCore;
using Project1_VTCA.Data;
using Project1_VTCA.DTOs;
using Project1_VTCA.Utils;
using Spectre.Console;
using System.Threading.Tasks;

namespace Project1_VTCA.Services
{
    public class AuthService : IAuthService
    {
        private readonly SneakerShopDbContext _context;
        private readonly ISessionService _sessionService;

        public AuthService(SneakerShopDbContext context, ISessionService sessionService)
        {
            _context = context;
            _sessionService = sessionService;
        }

        public async Task<AuthResult> LoginAsync()
        {
            AnsiConsole.Write(new Rule("[bold yellow]ĐĂNG NHẬP[/]").Centered());
            var username = AnsiConsole.Ask<string>("Nhập [green]Username[/]:");
            var password = AnsiConsole.Prompt(new TextPrompt<string>("Nhập [green]Password[/]:").Secret());

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == username);

            if (user == null || !PasswordHasher.VerifyPassword(password, user.PasswordHash))
            {
                return new AuthResult(false, "[red]Sai tên đăng nhập hoặc mật khẩu.[/]");
            }

            _sessionService.LoginUser(user);
            return new AuthResult(true, $"[green]Đăng nhập thành công! Chào mừng {user.FullName}[/]", user.Role);
        }

        public async Task<AuthResult> RegisterAsync()
        {
            AnsiConsole.Write(new Rule("[bold yellow]ĐĂNG KÝ TÀI KHOẢN MỚI[/]").Centered());
            AnsiConsole.MarkupLine("[dim](Nhập 'exit' bất cứ lúc nào để hủy bỏ)[/]\n");

            string username = ConsoleHelper.PromptForInput(
                "Nhập [green]Username[/] (viết liền, không dấu, 3-20 ký tự):",
                InputValidator.IsValidUsername,
                "Username không hợp lệ."
            );
            if (username == null) return new AuthResult(false, "[yellow]Đã hủy đăng ký.[/]");
            if (await _context.Users.AnyAsync(u => u.Username == username))
            {
                return new AuthResult(false, "[red]Username này đã tồn tại.[/]");
            }

            string password = ConsoleHelper.PromptForInput(
                "Nhập [green]Password[/] (6-20 ký tự):",
                InputValidator.IsValidPassword,
                "Mật khẩu phải từ 6 đến 20 ký tự."
            );
            if (password == null) return new AuthResult(false, "[yellow]Đã hủy đăng ký.[/]");

            string fullName = ConsoleHelper.PromptForInput("Nhập [green]Họ và Tên[/]:", f => !string.IsNullOrWhiteSpace(f), "Họ và tên không được để trống.");
            if (fullName == null) return new AuthResult(false, "[yellow]Đã hủy đăng ký.[/]");

            string email = ConsoleHelper.PromptForInput("Nhập [green]Email[/]:", InputValidator.IsValidEmail, "Email không hợp lệ.");
            if (email == null) return new AuthResult(false, "[yellow]Đã hủy đăng ký.[/]");

            string phoneNumber = ConsoleHelper.PromptForInput("Nhập [green]Số điện thoại[/]:", InputValidator.IsValidPhoneNumber, "Số điện thoại không hợp lệ (cần đủ 10 số).");
            if (phoneNumber == null) return new AuthResult(false, "[yellow]Đã hủy đăng ký.[/]");

            var gender = AnsiConsole.Prompt(new SelectionPrompt<string>().Title("Chọn [green]Giới tính[/]:").AddChoices(new[] { "Male", "Female", "Unisex" }));

            var newUser = new User
            {
                Username = username,
                PasswordHash = PasswordHasher.HashPassword(password),
                FullName = fullName,
                Email = email,
                PhoneNumber = phoneNumber,
                Gender = gender,
                Role = "Customer", // Luôn tạo là Customer
                IsActive = true
            };

            _context.Users.Add(newUser);
            await _context.SaveChangesAsync();

            return new AuthResult(true, "[bold green]Đăng ký tài khoản thành công![/]");
        }
    }
}