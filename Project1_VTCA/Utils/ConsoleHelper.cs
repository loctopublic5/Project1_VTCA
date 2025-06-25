using Spectre.Console;
using System;

namespace Project1_VTCA.Utils
{
    public static class ConsoleHelper
    {
        public static string PromptForInput(string promptTitle, Func<string, bool> validationRule, string errorMessage)
        {
            while (true)
            {
                var input = AnsiConsole.Ask<string>(promptTitle);
                if (input.Equals("exit", StringComparison.OrdinalIgnoreCase))
                {
                    return null;
                }
                if (validationRule(input))
                {
                    return input;
                }
                AnsiConsole.MarkupLine($"[red]{errorMessage}[/]");
            }
        }

        public static string PromptForPassword()
        {
            while (true)
            {
                var password = AnsiConsole.Prompt(
                    new TextPrompt<string>("Nhập [green]Password[/]:")
                        .PromptStyle("red")
                        .Secret());

                if (password.Equals("exit", StringComparison.OrdinalIgnoreCase)) return null;

                var confirmPassword = AnsiConsole.Prompt(
                    new TextPrompt<string>("Xác nhận [green]Password[/]:")
                        .PromptStyle("red")
                        .Secret());

                if (confirmPassword.Equals("exit", StringComparison.OrdinalIgnoreCase)) return null;

                if (password == confirmPassword)
                {
                    return password;
                }

                AnsiConsole.MarkupLine("[red]Mật khẩu xác nhận không khớp. Vui lòng nhập lại.[/]");
            }
        }
    }
}