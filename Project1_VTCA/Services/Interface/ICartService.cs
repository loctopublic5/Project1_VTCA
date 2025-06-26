using Project1_VTCA.Data;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Project1_VTCA.Services.Interface
{
    public interface ICartService
    {
        Task<string> AddToCartAsync(int userId, int productId, int size, int quantity);
        Task<List<CartItem>> GetCartItemsAsync(int userId);
        Task UpdateCartItemQuantityAsync(int cartItemId, int newQuantity);
        Task RemoveCartItemAsync(int cartItemId);
        Task ClearCartAsync(int userId);
    }
}