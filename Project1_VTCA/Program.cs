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

        var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
        builder.Services.AddDbContext<SneakerShopDbContext>(options =>
            options.UseSqlServer(connectionString, sqlOpt => sqlOpt.EnableRetryOnFailure()));

        builder.Services.AddScoped<IAuthService, AuthService>();
        builder.Services.AddScoped<IProductService, ProductService>();

        builder.Services.AddSingleton<ConsoleLayout>();
        builder.Services.AddTransient<MainMenu>();
        builder.Services.AddTransient<ProductMenu>();

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
                Console.WriteLine($"Đã xảy ra lỗi nghiêm trọng: {ex.Message}");
                Console.ReadKey();
            }
        }
    }

    static async Task ApplyMigrations(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<SneakerShopDbContext>();
        await dbContext.Database.MigrateAsync();
    }
}