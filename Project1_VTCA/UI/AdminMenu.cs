using System;
using System.Threading.Tasks;
using Project1_VTCA.UI.Interface;
namespace Project1_VTCA.UI { 
    public class AdminMenu : IAdminMenu 
    { 
        public async Task Show() 
        {
            Console.WriteLine("Đây là menu của Admin. Sẽ được hiện thực sau."); await Task.CompletedTask; 
        } 
    } 
}