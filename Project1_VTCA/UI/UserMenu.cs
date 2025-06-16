using System;
using System.Threading.Tasks;
namespace Project1_VTCA.UI {
    public class UserMenu : IUserMenu 
    { 
        public async Task Show() { Console.WriteLine("Đây là menu của Customer. Sẽ được hiện thực sau.");
            await Task.CompletedTask; } 
    } 
}