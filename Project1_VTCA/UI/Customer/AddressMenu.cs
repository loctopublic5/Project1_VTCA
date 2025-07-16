using Project1_VTCA.Data;
using Project1_VTCA.Services.Interface;
using Project1_VTCA.UI.Customer.Interfaces;
using Project1_VTCA.Utils;
using Spectre.Console;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Project1_VTCA.UI.Customer
{
    public class AddressMenu : IAddressMenu
    {
        private readonly IAddressService _addressService;
        private readonly ISessionService _sessionService;

        public AddressMenu(IAddressService addressService, ISessionService sessionService)
        {
            _addressService = addressService;
            _sessionService = sessionService;
        }

        public async Task ShowAddressManagementAsync()
        {
            while (true)
            {
                AnsiConsole.Clear();
                var addresses = await _addressService.GetActiveAddressesAsync(_sessionService.CurrentUser.UserID);
                AnsiConsole.Write(CreateAddressTable(addresses));

                var choice = AnsiConsole.Prompt(
                    new SelectionPrompt<string>()
                        .Title("\n[bold yellow]TÙY CHỌN ĐỊA CHỈ[/]")
                        .AddChoices(new[] {
                            "Thêm địa chỉ mới", "Sửa địa chỉ", "Xóa địa chỉ", "Đặt làm mặc định", "Quay lại"
                        })
                );

                switch (choice)
                {
                    case "Thêm địa chỉ mới": await HandleAddAddressFlowAsync(); break;
                    case "Sửa địa chỉ": await HandleUpdateAddress(addresses); break;
                    case "Xóa địa chỉ": await HandleDeleteAddress(addresses); break;
                    case "Đặt làm mặc định": await HandleSetDefault(addresses); break;
                    case "Quay lại": return;
                }
            }
        }
        
        private async Task HandleUpdateAddress(List<Address> addresses)
        {
            if (!addresses.Any())
            {
                AnsiConsole.MarkupLine("[red]Không có địa chỉ để sửa.[/]");
                Console.ReadKey();
                return;
            }

            var idToUpdate = AnsiConsole.Ask<int>("Nhập [green]ID[/] địa chỉ muốn sửa (hoặc 0 để quay lại):");
            if (idToUpdate == 0) return;

            var addressToUpdate = addresses.FirstOrDefault(a => a.AddressID == idToUpdate);
            if (addressToUpdate == null)
            {
                AnsiConsole.MarkupLine("[red]ID không hợp lệ.[/]");
                Console.ReadKey();
                return;
            }

            AnsiConsole.MarkupLine("[dim]Hướng dẫn: Nhấn Enter để giữ lại thông tin cũ.[/]");

          
            var newAddressDetail = AnsiConsole.Prompt(new TextPrompt<string>($"Nhập [green]Địa chỉ chi tiết mới[/]:").AllowEmpty().DefaultValue(addressToUpdate.AddressDetail));
            addressToUpdate.AddressDetail = string.IsNullOrEmpty(newAddressDetail) ? addressToUpdate.AddressDetail : newAddressDetail;

          
            var newCity = AnsiConsole.Prompt(new TextPrompt<string>($"Nhập [green]Tỉnh/Thành phố mới[/]:").AllowEmpty().DefaultValue(addressToUpdate.City));
            addressToUpdate.City = string.IsNullOrEmpty(newCity) ? addressToUpdate.City : newCity;

       
            var newPhone = AnsiConsole.Prompt(new TextPrompt<string>($"Nhập [green]SĐT mới[/]:").AllowEmpty().DefaultValue(addressToUpdate.ReceivePhone));
            if (!string.IsNullOrEmpty(newPhone))
            {
                while (!InputValidator.IsValidPhoneNumber(newPhone))
                {
                    newPhone = AnsiConsole.Prompt(new TextPrompt<string>("[red]SĐT không hợp lệ.[/] Nhập lại SĐT mới:").AllowEmpty().DefaultValue(addressToUpdate.ReceivePhone));
                    if (string.IsNullOrEmpty(newPhone)) break; // Người dùng quyết định giữ lại số cũ
                }
                addressToUpdate.ReceivePhone = string.IsNullOrEmpty(newPhone) ? addressToUpdate.ReceivePhone : newPhone;
            }

            var response = await _addressService.UpdateAddressAsync(addressToUpdate);
            AnsiConsole.MarkupLine($"\n[green]{Markup.Escape(response.Message)}[/]");
            Console.ReadKey();
        }

       
        #region Other AddressMenu Methods
        private Table CreateAddressTable(List<Address> addresses)
        {
            var table = new Table().Expand().Border(TableBorder.Rounded);
            table.Title = new TableTitle("DANH SÁCH ĐỊA CHỈ CỦA BẠN");
            table.AddColumn("ID");
            table.AddColumn("Địa chỉ chi tiết");
            table.AddColumn("Tỉnh/Thành");
            table.AddColumn("SĐT Nhận hàng");
            table.AddColumn("Mặc định");

            if (!addresses.Any())
            {
                table.AddRow("-", "[grey]Bạn chưa có địa chỉ nào.[/]", "-", "-", "-");
                return table;
            }

            foreach (var addr in addresses)
            {
                var isDefaultMarkup = addr.IsDefault ? "[bold green]✔[/]" : "";
                table.AddRow(
                    addr.AddressID.ToString(),
                    Markup.Escape(addr.AddressDetail),
                    Markup.Escape(addr.City),
                    addr.ReceivePhone,
                    isDefaultMarkup
                );
            }
            return table;
        }

        public async Task<Address?> HandleAddAddressFlowAsync(bool setDefault = false)
        {
            AnsiConsole.MarkupLine("\n[bold yellow]--- THÊM ĐỊA CHỈ MỚI ---[/]");
            string addressDetail = AnsiConsole.Ask<string>("Nhập [green]Địa chỉ chi tiết[/] (số nhà, đường...):");
            string city = AnsiConsole.Ask<string>("Nhập [green]Tỉnh/Thành phố[/]:");
            string userPhone = _sessionService.CurrentUser.PhoneNumber;
            string phone = AnsiConsole.Prompt(
                new TextPrompt<string>($"Nhập SĐT người nhận (hoặc nhấn [yellow]Enter[/] để dùng SĐT: [underline]{userPhone}[/]):")
                    .AllowEmpty()
            );
            if (string.IsNullOrEmpty(phone)) phone = userPhone;
            else
            {
                while (!InputValidator.IsValidPhoneNumber(phone))
                {
                    phone = ConsoleHelper.PromptForInput("[red]SĐT không hợp lệ.[/] Nhập lại:", InputValidator.IsValidPhoneNumber, "SĐT không hợp lệ.");
                    if (phone == null) return null;
                }
            }

            bool isDefault = setDefault || AnsiConsole.Confirm("Đặt địa chỉ này làm mặc định?");

            var newAddress = new Address
            {
                UserID = _sessionService.CurrentUser.UserID,
                AddressDetail = addressDetail,
                City = city,
                ReceivePhone = phone,
                IsDefault = isDefault
            };

            var (response, createdAddress) = await _addressService.AddAddressAsync(newAddress);
            AnsiConsole.MarkupLine($"[green]{Markup.Escape(response.Message)}[/]");
            Console.ReadKey();
            return createdAddress;
        }

        private async Task HandleDeleteAddress(List<Address> addresses)
        {
            if (!addresses.Any()) { AnsiConsole.MarkupLine("[red]Không có địa chỉ để xóa.[/]"); Console.ReadKey(); return; }
            var idToDelete = AnsiConsole.Ask<int>("Nhập [green]ID[/] địa chỉ muốn xóa:");

            if (addresses.All(a => a.AddressID != idToDelete)) { AnsiConsole.MarkupLine("[red]ID không hợp lệ.[/]"); Console.ReadKey(); return; }

            if (AnsiConsole.Confirm($"[bold red]Bạn có chắc muốn xóa địa chỉ có ID {idToDelete} không?[/]"))
            {
                var response = await _addressService.SoftDeleteAddressAsync(idToDelete, _sessionService.CurrentUser.UserID);
                string color = response.IsSuccess ? "green" : "red";
                AnsiConsole.MarkupLine($"[{color}]{Markup.Escape(response.Message)}[/]");
                Console.ReadKey();
            }
        }

        private async Task HandleSetDefault(List<Address> addresses)
        {
            if (!addresses.Any()) { AnsiConsole.MarkupLine("[red]Không có địa chỉ để thao tác.[/]"); Console.ReadKey(); return; }
            var idToSet = AnsiConsole.Ask<int>("Nhập [green]ID[/] địa chỉ muốn đặt làm mặc định:");

            if (addresses.All(a => a.AddressID != idToSet)) { AnsiConsole.MarkupLine("[red]ID không hợp lệ.[/]"); Console.ReadKey(); return; }

            var response = await _addressService.SetDefaultAddressAsync(idToSet, _sessionService.CurrentUser.UserID);
            AnsiConsole.MarkupLine($"[green]{Markup.Escape(response.Message)}[/]");
            Console.ReadKey();
        }
        #endregion
    }
}