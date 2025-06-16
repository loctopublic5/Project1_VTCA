using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Project1_VTCA.Data;
using Project1_VTCA.DTOs;
using Project1_VTCA.Services;
using Project1_VTCA.UI;
using Spectre.Console;
using System;
using System.Linq;
using System.Threading.Tasks;

class Program
{
    static async Task Main(string[] args)
    {
        Console.OutputEncoding = System.Text.Encoding.UTF8;
        Console.InputEncoding = System.Text.Encoding.UTF8;

        var builder = Host.CreateApplicationBuilder(args);

        var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
        builder.Services.AddDbContext<SneakerShopDbContext>(options =>
            options.UseSqlServer(connectionString, sqlOpt => sqlOpt.EnableRetryOnFailure()));

        builder.Services.AddSingleton<ISessionService, SessionService>();
        builder.Services.AddScoped<IAuthService, AuthService>();
        builder.Services.AddScoped<IProductService, ProductService>();

        builder.Services.AddSingleton<ConsoleLayout>();
        builder.Services.AddTransient<MainMenu>();
        builder.Services.AddTransient<ProductMenu>();
        builder.Services.AddTransient<IUserMenu, UserMenu>();
        builder.Services.AddTransient<IAdminMenu, AdminMenu>();

        using var host = builder.Build();

        await ApplyMigrations(host.Services);

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
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"\nĐÃ CÓ LỖI XẢY RA: {ex.Message}");
                Console.ResetColor();
                Console.WriteLine("Nhấn phím bất kỳ để thoát.");
                Console.ReadKey();
            }
        }
    }

    static async Task ApplyMigrations(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<SneakerShopDbContext>();
        if ((await dbContext.Database.GetPendingMigrationsAsync()).Any())
        {
            AnsiConsole.MarkupLine("[yellow]Đang kiểm tra và cập nhật cơ sở dữ liệu...[/]");
            await dbContext.Database.MigrateAsync();
            AnsiConsole.MarkupLine("[green]Cơ sở dữ liệu đã ở trạng thái mới nhất.[/]");
            await Task.Delay(1500);
        }
    }
}