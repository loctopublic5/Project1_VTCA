using Project1_VTCA.Data;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Project1_VTCA.Services.Interface
{
    public interface IOrderService
    {
        // Customer-specific methods
        Task<ServiceResponse> CreateOrderAsync(int userId, List<CartItem> items, string shippingAddress, string shippingPhone, string paymentMethod);
        decimal CalculateShippingFee(int totalQuantity);

        Task<(List<Order> Orders, int TotalPages)> GetOrdersAsync(int userId, string? statusFilter, int pageNumber, int pageSize);

        Task<Order?> GetOrderByIdAsync(int orderId, int userId);
        Task<ServiceResponse> RequestCancellationAsync(int userId, int orderId, string reason);

        Task<ServiceResponse> ApproveCancellationAsync(int orderId);
        Task<ServiceResponse> ConfirmOrderAsync(int orderId);

        // Admin-specific methods
        Task<(List<Order> Orders, int TotalPages)> GetOrdersForAdminAsync(string? statusFilter, int pageNumber, int pageSize);

        Task<ServiceResponse> ConfirmOrderAsync(int orderId, int adminId);
        Task<ServiceResponse> RejectOrderAsync(int orderId, int adminId, string reason);
        Task<ServiceResponse> ApproveCancellationAsync(int orderId, int adminId);
    }
}