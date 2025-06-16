using Project1_VTCA.DTOs;
using System.Threading.Tasks;

namespace Project1_VTCA.Services
{
    public interface IAuthService
    {
        Task<AuthResult> LoginAsync();
        Task<AuthResult> RegisterAsync();
    }
}