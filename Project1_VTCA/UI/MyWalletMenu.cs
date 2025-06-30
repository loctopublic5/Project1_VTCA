using Project1_VTCA.Services.Interface;
using Project1_VTCA.UI.Interface;
using Spectre.Console;
using System.Threading.Tasks;

namespace Project1_VTCA.UI
{
    public class MyWalletMenu : IMyWalletMenu
    {
        private readonly IUserService _userService;
        private readonly ISessionService _sessionService;

        public MyWalletMenu(IUserService userService, ISessionService sessionService)
        {
            _userService = userService;
            _sessionService = sessionService;
        }

        public async Task ShowWalletAsync()
        {
            while (true)
            {
                AnsiConsole.Clear();
                AnsiConsole.MarkupLine($"Số dư hiện tại của bạn: [bold yellow]{_sessionService.CurrentUser.Balance:N0} VNĐ[/]");
                AnsiConsole.Write(new Rule());

                var choice = AnsiConsole.Prompt(
                    new SelectionPrompt<string>()
                        .Title("\n[bold]Mời bạn chọn mệnh giá muốn nạp:[/]")
                        .AddChoices(new[] {
                            "Nạp 100,000 VNĐ",
                            "Nạp 200,000 VNĐ",
                            "Nạp 500,000 VNĐ",
                            "Nhập một số tiền khác...",
                            "Quay lại"
                        })
                );

                decimal amount = 0;
                bool shouldExit = false;

                switch (choice)
                {
                    case "Nạp 100,000 VNĐ": amount = 100000; break;
                    case "Nạp 200,000 VNĐ": amount = 200000; break;
                    case "Nạp 500,000 VNĐ": amount = 500000; break;
                    case "Nhập một số tiền khác...":
                        amount = AnsiConsole.Ask<decimal>("Nhập [green]số tiền[/] bạn muốn nạp (từ 10,000 đến 10,000,000, hoặc nhập 0 để quay lại):");
                        if (amount == 0) continue;
                        break;
                    case "Quay lại":
                        shouldExit = true;
                        break;
                }

                if (shouldExit) break;

                if (amount > 0)
                {
                    await ProcessDeposit(amount);
                }
            }
        }

        private async Task ProcessDeposit(decimal amount)
        {
            if (amount < 10000 || amount > 10000000)
            {
                AnsiConsole.MarkupLine("[red]Lỗi: Số tiền nạp phải từ 10.000 VNĐ đến 10.000.000 VNĐ.[/]");
                Console.ReadKey();
                return;
            }

            if (!AnsiConsole.Confirm($"Bạn có chắc chắn muốn nạp [yellow]{amount:N0} VNĐ[/]?"))
            {
                AnsiConsole.MarkupLine("[yellow]Đã hủy giao dịch.[/]");
                Console.ReadKey();
                return;
            }

            var response = await _userService.DepositAsync(_sessionService.CurrentUser.UserID, amount);

            var panel = new Panel(new Markup($"[bold {(response.IsSuccess ? "green" : "red")}]{Markup.Escape(response.Message)}[/]"))
                .Header("THÔNG BÁO GIAO DỊCH")
                .Border(BoxBorder.Rounded);

            AnsiConsole.Write(panel);

            if (response.IsSuccess)
            {
                _sessionService.CurrentUser.Balance += amount;
            }

            AnsiConsole.MarkupLine("[dim]Nhấn phím bất kỳ để tiếp tục...[/]");
            Console.ReadKey();
        }
    }
}