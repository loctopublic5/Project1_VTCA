using Microsoft.EntityFrameworkCore;
using Project1_VTCA.Data;
using Project1_VTCA.DTOs;
using Project1_VTCA.Utils;
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

        public async Task<AuthResult> LoginAsync(string username, string password)
        {
            var user = await _context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Username == username);
            if (user == null || !PasswordHasher.VerifyPassword(password, user.PasswordHash))
            {
                return new AuthResult(false, "[red]Sai tên đăng nhập hoặc mật khẩu.[/]");
            }
            _sessionService.LoginUser(user);
            return new AuthResult(true, $"[bold green]Đăng nhập thành công! Chào mừng {user.FullName}[/]", user.Role);
        }

        public async Task<AuthResult> RegisterAsync(UserRegistrationDto data)
        {
            if (await _context.Users.AnyAsync(u => u.Username == data.Username))
            {
                return new AuthResult(false, "[red]Username này đã tồn tại.[/]");
            }
            var newUser = new User
            {
                Username = data.Username,
                PasswordHash = PasswordHasher.HashPassword(data.Password),
                FullName = data.FullName,
                Email = data.Email,
                PhoneNumber = data.PhoneNumber,
                Gender = data.Gender,
                Role = "Customer",
                IsActive = true,
                Balance = 0,
                TotalSpending = 0
            };
            _context.Users.Add(newUser);
            await _context.SaveChangesAsync();
            return new AuthResult(true, "[bold green]Đăng ký tài khoản thành công![/]");
        }

        public async Task<AuthResult> ForgotPasswordAsync(string username, string email, string newPassword)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == username);
            if (user == null)
            {
                return new AuthResult(false, "[red]Không tìm thấy tài khoản với username này.[/]");
            }
            if (user.Email?.Equals(email, System.StringComparison.OrdinalIgnoreCase) != true)
            {
                return new AuthResult(false, "[red]Email không khớp với tài khoản.[/]");
            }
            user.PasswordHash = PasswordHasher.HashPassword(newPassword);
            await _context.SaveChangesAsync();
            return new AuthResult(true, "[bold green]Cập nhật mật khẩu thành công! Vui lòng đăng nhập lại.[/]");
        }
    }
}