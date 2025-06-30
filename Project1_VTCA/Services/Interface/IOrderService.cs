using Project1_VTCA.Data;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Project1_VTCA.Services.Interface
{
    public interface IOrderService
    {
        Task<ServiceResponse> CreateOrderAsync(int userId, List<CartItem> items, string shippingAddress, string shippingPhone, string paymentMethod);
        decimal CalculateShippingFee(int totalQuantity);

        // NÂNG CẤP: Thay đổi chữ ký để hỗ trợ phân trang
        Task<(List<Order> Orders, int TotalPages)> GetOrdersAsync(int userId, string? statusFilter, int pageNumber, int pageSize);

        Task<Order?> GetOrderByIdAsync(int orderId, int userId);
        Task<ServiceResponse> RequestCancellationAsync(int userId, int orderId, string reason);

        Task<ServiceResponse> ApproveCancellationAsync(int orderId);
        Task<ServiceResponse> ConfirmOrderAsync(int orderId);
    }
}