using System;
using System.Threading.Tasks;
namespace Project1_VTCA.UI { 
    public class AdminMenu : IAdminMenu 
    { 
        public async Task Show() 
        {
            Console.WriteLine("Đây là menu của Admin. Sẽ được hiện thực sau."); await Task.CompletedTask; 
        } 
    } 
}