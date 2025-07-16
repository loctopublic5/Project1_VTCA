using Microsoft.EntityFrameworkCore;
using Project1_VTCA.Data;
using Project1_VTCA.DTOs;
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

        public async Task<BalanceUpdateResult> DepositAsync(int userId, decimal amount)
        {
            if (amount <= 0)
            {
                return new BalanceUpdateResult(false, "Số tiền nạp phải lớn hơn 0.", 0);
            }

            var user = await _context.Users.FindAsync(userId);
            if (user == null)
            {
                return new BalanceUpdateResult(false, "Không tìm thấy người dùng.", 0);
            }

            user.Balance += amount;
            await _context.SaveChangesAsync();

            return new BalanceUpdateResult(true, "Giao dịch thành công!", user.Balance);
        }

        public async Task<(List<User> Customers, int TotalPages)> GetCustomerStatisticsAsync(string sortBy, int pageNumber, int pageSize)
        {
            var customersQuery = _context.Users
                .Where(u => u.Role == "Customer")
                .Include(u => u.Orders);

     
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