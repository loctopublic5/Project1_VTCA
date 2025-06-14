using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Project1_VTCA.Data;
using Project1_VTCA.Services;
using Project1_VTCA.UI;
using System;
using System.Threading.Tasks;

class Program
{
    static async Task Main(string[] args)
    {
        Console.OutputEncoding = System.Text.Encoding.UTF8;
        Console.InputEncoding = System.Text.Encoding.UTF8;

        var builder = Host.CreateApplicationBuilder(args);

        // Đăng ký DbContext với tên đã cập nhật
        var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
        builder.Services.AddDbContext<SneakerShopDbContext>(options =>
            options.UseSqlServer(connectionString, sqlOpt => sqlOpt.EnableRetryOnFailure()));

        // Đăng ký các Services
        builder.Services.AddScoped<IAuthService, AuthService>();
        builder.Services.AddScoped<IProductService, ProductService>();

        // Đăng ký các lớp UI và Helper
        builder.Services.AddSingleton<ConsoleLayout>();
        builder.Services.AddTransient<MainMenu>();

        using var host = builder.Build();

        using (var scope = host.Services.CreateScope())
        {
            var services = scope.ServiceProvider;
            try
            {
                var mainMenu = services.GetRequiredService<MainMenu>();
                await mainMenu.Show();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Đã xảy ra lỗi nghiêm trọng: {ex.Message}");
                Console.WriteLine(ex.InnerException?.Message);
            }
        }
    }
}