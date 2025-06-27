using Project1_VTCA.Data;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Project1_VTCA.Services.Interface
{
    public interface ICartService
    {
        Task<ServiceResponse> AddToCartAsync(int userId, int productId, int size, int quantity);
        Task<List<CartItem>> GetCartItemsAsync(int userId);
        Task<ServiceResponse> UpdateCartItemQuantityAsync(int cartItemId, int newQuantity);
        Task<ServiceResponse> UpdateCartItemSizeAsync(int userId, int cartItemId, int newSize);
        Task RemoveCartItemsAsync(int userId, List<int> cartItemIds);
        Task ClearCartAsync(int userId);
    }
}