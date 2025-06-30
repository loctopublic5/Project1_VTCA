using Project1_VTCA.Data;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Project1_VTCA.Services.Interface
{
    public interface IOrderService
    {
        Task<ServiceResponse> CreateOrderAsync(int userId, List<CartItem> items, string shippingAddress, string shippingPhone, string paymentMethod);
        decimal CalculateShippingFee(int totalQuantity);


        Task<List<Order>> GetOrdersAsync(int userId, string? statusFilter = null);
        Task<ServiceResponse> RequestCancellationAsync(int userId, int orderId, string reason);
        Task<ServiceResponse> ApproveCancellationAsync(int orderId); // Cho Giai đoạn 4
        Task<ServiceResponse> ConfirmOrderAsync(int orderId); // Cho Giai đoạn 4
    }
}