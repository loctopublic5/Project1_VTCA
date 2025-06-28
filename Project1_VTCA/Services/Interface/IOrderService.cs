using Project1_VTCA.Data;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Project1_VTCA.Services.Interface
{
    public interface IOrderService
    {
        Task<ServiceResponse> CreateOrderAsync(int userId, List<CartItem> items, string shippingAddress, string paymentMethod);
        decimal CalculateShippingFee(int totalQuantity);
    }
}