using Microsoft.EntityFrameworkCore;
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

        public async Task<(List<User> Customers, int TotalPages)> GetCustomerStatisticsAsync(string sortBy, int pageNumber, int pageSize)
        {
            var customersQuery = _context.Users
                .Where(u => u.Role == "Customer")
                .Include(u => u.Orders);

            // Materialize Include before ordering to avoid type mismatch
            var customersQueryBase = _context.Users
                .Where(u => u.Role == "Customer")
                .Include(u => u.Orders)
                .AsQueryable();

            IQueryable<User> orderedQuery;
            switch (sortBy)
            {
                case "spending_desc":
                    orderedQuery = customersQueryBase.OrderByDescending(u => u.TotalSpending);
                    break;
                case "spending_asc":
                    orderedQuery = customersQueryBase.OrderBy(u => u.TotalSpending);
                    break;
                default:
                    orderedQuery = customersQueryBase.OrderBy(u => u.UserID);
                    break;
            }

            var totalCustomers = await customersQuery.CountAsync();
            var totalPages = (int)Math.Ceiling(totalCustomers / (double)pageSize);

            var customers = await customersQuery
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (customers, totalPages);
        }

        
    }
}