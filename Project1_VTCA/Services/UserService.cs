using Project1_VTCA.Data;
using Project1_VTCA.Services.Interface;
using System.Threading.Tasks;

namespace Project1_VTCA.Services
{
    public class UserService : IUserService
    {
        private readonly SneakerShopDbContext _context;

        public UserService(SneakerShopDbContext context)
        {
            _context = context;
        }

        public async Task<ServiceResponse> DepositAsync(int userId, decimal amount)
        {
            if (amount <= 0)
            {
                return new ServiceResponse(false, "Số tiền nạp phải lớn hơn 0.");
            }

            var user = await _context.Users.FindAsync(userId);
            if (user == null)
            {
                return new ServiceResponse(false, "Không tìm thấy người dùng.");
            }

            user.Balance += amount;
            await _context.SaveChangesAsync();

            return new ServiceResponse(true, $"Nạp tiền thành công! Số dư mới của bạn là {user.Balance:N0} VNĐ.");
        }
    }
}