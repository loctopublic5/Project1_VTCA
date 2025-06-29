using System.Threading.Tasks;

namespace Project1_VTCA.Services.Interface
{
    public interface IUserService
    {
        Task<ServiceResponse> DepositAsync(int userId, decimal amount);
    }
}