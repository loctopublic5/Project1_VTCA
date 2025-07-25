﻿using Project1_VTCA.Services.Interface;
using Project1_VTCA.UI.Admin.Interface;
using Project1_VTCA.UI.Customer.Interfaces;
using Spectre.Console;
using System.Threading.Tasks;

namespace Project1_VTCA.UI.Admin
{
    public class AdminMenu : IAdminMenu
    {
        private readonly IAdminOrderMenu _adminOrderMenu;
        private readonly IAdminCustomerMenu _adminCustomerMenu;
        private readonly IAdminProductMenu _adminProductMenu;
        private readonly ISessionService _sessionService;

        public AdminMenu(IAdminOrderMenu adminOrderMenu, IAdminCustomerMenu adminCustomerMenu,IAdminProductMenu adminProductMenu, ISessionService sessionService)
        {
            _adminOrderMenu = adminOrderMenu;
            _adminCustomerMenu = adminCustomerMenu;
            _adminProductMenu = adminProductMenu;
            _sessionService = sessionService;
        }

        public async Task Show()
        {
            while (true)
            {
                AnsiConsole.Clear();
                AnsiConsole.Write(new Rule("[bold red]ADMIN DASHBOARD[/]").Centered());

                var choice = AnsiConsole.Prompt(
                    new SelectionPrompt<string>()
                    .Title("\n[bold]Chọn một chức năng quản trị:[/]")
                    .AddChoices(new[] {
                        "Quản lý Đơn hàng",
                        "Quản lý Sản phẩm ",
                        "Quản lý Khách hàng ",
                        "[red]Đăng xuất[/]"
                    })
                );

                switch (choice)
                {
                    case "Quản lý Đơn hàng":
                        await _adminOrderMenu.ShowAsync();
                        break;
                    case "Quản lý Sản phẩm ":
                        await _adminProductMenu.ShowAsync();
                        break;
                    case "Quản lý Khách hàng ":
                        await _adminCustomerMenu.ShowAsync(); 
                        break;
                        break;
                    case "[red]Đăng xuất[/]":
                        _sessionService.LogoutUser();
                        AnsiConsole.MarkupLine("\n[green]Bạn đã đăng xuất khỏi tài khoản Admin.[/]");
                        Console.ReadKey();
                        return; 
                }
            }
        }
    }
}