using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Project1_VTCA.Data;
using Project1_VTCA.Services;
using Project1_VTCA.Services.Interface;
using Project1_VTCA.UI.Admin.Interface;
using Project1_VTCA.UI.Admin;
using Project1_VTCA.UI.Draw;
using Spectre.Console;
using System;
using System.Linq;
using System.Threading.Tasks;
using Project1_VTCA.UI.Customer.Interface;
using Project1_VTCA.UI.Customer;
using Project1_VTCA.UI;

class Program
{
    static async Task Main(string[] args)
    {
        Console.OutputEncoding = System.Text.Encoding.UTF8;
        Console.InputEncoding = System.Text.Encoding.UTF8;

        var builder = Host.CreateApplicationBuilder(args);

        var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

        builder.Services.AddDbContext<SneakerShopDbContext>(options =>
            options.UseSqlServer(connectionString, sqlOptions =>
            {
                sqlOptions.EnableRetryOnFailure();
            }));

        // ĐĂNG KÝ TOÀN BỘ CÁC SERVICES
        builder.Services.AddSingleton<ISessionService, SessionService>();
        builder.Services.AddScoped<IAuthService, AuthService>();
        builder.Services.AddScoped<IProductService, ProductService>();
        builder.Services.AddScoped<IPromotionService, PromotionService>();
        builder.Services.AddScoped<ICartService, CartService>();
        builder.Services.AddScoped<IOrderService, OrderService>();
        builder.Services.AddScoped<IUserService, UserService>();
        builder.Services.AddScoped<IAddressService, AddressService>();

        // ĐĂNG KÝ TOÀN BỘ CÁC MENUS
        builder.Services.AddSingleton<ConsoleLayout>();
        builder.Services.AddTransient<MainMenu>();
        builder.Services.AddTransient<ProductMenu>();
        builder.Services.AddTransient<IUserMenu, UserMenu>();
        builder.Services.AddTransient<IAdminMenu, AdminMenu>();
        builder.Services.AddTransient<ICartMenu, CartMenu>();
        builder.Services.AddTransient<ICheckoutMenu, CheckoutMenu>();
        builder.Services.AddTransient<IAddressMenu, AddressMenu>();
        builder.Services.AddTransient<IMyWalletMenu, MyWalletMenu>(); 
        builder.Services.AddTransient<IOrderHistoryMenu, OrderHistoryMenu>();
        builder.Services.AddTransient<IAccountManagementMenu, AccountManagementMenu>();
        builder.Services.AddTransient<IAdminOrderMenu, AdminOrderMenu>();


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
                AnsiConsole.Clear();
                AnsiConsole.Write(new Rule("[bold red]ĐÃ CÓ LỖI NGHIÊM TRỌNG XẢY RA[/]").Centered());
                AnsiConsole.WriteLine();

                int exceptionCount = 1;
                Exception currentEx = ex;

                while (currentEx != null)
                {
                    var panel = new Panel(
                        new Rows(
                            new Markup($"[bold yellow]Loại lỗi:[/] [white]{Markup.Escape(currentEx.GetType().FullName)}[/]"),
                            new Markup($"[bold yellow]Thông báo:[/] [white]{Markup.Escape(currentEx.Message)}[/]")
                        ))
                        .Header(exceptionCount == 1 ? "Lỗi chính (Lớp ngoài cùng)" : $"Lỗi nội tại (Inner Exception) #{exceptionCount - 1}")
                        .Border(BoxBorder.Rounded)
                        .Expand();

                    AnsiConsole.Write(panel);

                    AnsiConsole.MarkupLine("\n[bold underline yellow]Dấu vết ngăn xếp (Stack Trace):[/]");
                    AnsiConsole.MarkupLine($"[grey]{Markup.Escape(currentEx.StackTrace ?? "Không có stack trace.")}[/]");
                    AnsiConsole.WriteLine();

                    currentEx = currentEx.InnerException;
                    exceptionCount++;

                    if (currentEx != null)
                    {
                        AnsiConsole.Write(new Rule("[red]Nguyên nhân gốc rễ (xem lỗi bên dưới)[/]").Centered());
                    }
                }

                AnsiConsole.Write(new Rule("[bold red]CHƯƠNG TRÌNH SẼ DỪNG LẠI[/]").Centered());
                AnsiConsole.MarkupLine("\n[dim]Nhấn phím bất kỳ để thoát.[/]");
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