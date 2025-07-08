using System.Threading.Tasks;
using Project1_VTCA.DTOs;
using Project1_VTCA.Data;

namespace Project1_VTCA.Services.Interface
{
    public interface IUserService
    {
        Task<BalanceUpdateResult> DepositAsync(int userId, decimal amount);
        Task<(List<User> Customers, int TotalPages)> GetCustomerStatisticsAsync(string sortBy, int pageNumber, int pageSize);
    }
}