using Microsoft.EntityFrameworkCore;
using Project1_VTCA.Data;
using Project1_VTCA.Services.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Project1_VTCA.Services
{
    public class CartService : ICartService
    {
        private readonly SneakerShopDbContext _context;

        public CartService(SneakerShopDbContext context)
        {
            _context = context;
        }

        public async Task<string> AddToCartAsync(int userId, int productId, int size, int quantity)
        {
            var productSize = await _context.ProductSizes
                .AsNoTracking()
                .FirstOrDefaultAsync(ps => ps.ProductID == productId && ps.Size == size);

            if (productSize == null)
            {
                return "[red]Lỗi: Size này không tồn tại cho sản phẩm.[/]";
            }

            int stock = productSize.QuantityInStock ?? 0;

            if (stock < quantity)
            {
                return $"[red]Lỗi: Không đủ số lượng tồn kho cho size {size}. Chỉ còn lại {stock}.[/]";
            }

            var existingCartItem = await _context.CartItems
                .FirstOrDefaultAsync(ci => ci.UserID == userId && ci.ProductID == productId && ci.Size == size);

            if (existingCartItem != null)
            {
                if (existingCartItem.Quantity + quantity > stock)
                {
                    return $"[red]Lỗi: Tổng số lượng trong giỏ ({existingCartItem.Quantity}) và số lượng thêm vào ({quantity}) vượt quá tồn kho ({stock}).[/]";
                }
                existingCartItem.Quantity += quantity;
            }
            else
            {
                var newCartItem = new CartItem
                {
                    UserID = userId,
                    ProductID = productId,
                    Size = size,
                    Quantity = quantity
                };
                _context.CartItems.Add(newCartItem);
            }

            await _context.SaveChangesAsync();
            return "[bold green]Đã thêm sản phẩm vào giỏ hàng thành công![/]";
        }

        public async Task<List<CartItem>> GetCartItemsAsync(int userId)
        {
            return await _context.CartItems
                .Where(ci => ci.UserID == userId)
                .Include(ci => ci.Product)
                .ThenInclude(p => p.ProductCategories)
                .ThenInclude(pc => pc.Category)
                .OrderBy(ci => ci.CartItemID)
                .ToListAsync();
        }

        public async Task UpdateCartItemQuantityAsync(int cartItemId, int newQuantity)
        {
            var cartItem = await _context.CartItems
                .Include(ci => ci.Product)
                .ThenInclude(p => p.ProductSizes)
                .FirstOrDefaultAsync(ci => ci.CartItemID == cartItemId);

            if (cartItem == null) throw new InvalidOperationException("Cart item not found.");

            if (newQuantity <= 0)
            {
                _context.CartItems.Remove(cartItem);
                await _context.SaveChangesAsync();
                return;
            }

            var productSize = cartItem.Product.ProductSizes.FirstOrDefault(ps => ps.Size == cartItem.Size);
            int stock = productSize?.QuantityInStock ?? 0;

            if (newQuantity > stock)
            {
                throw new InvalidOperationException($"Stock for size {cartItem.Size} is only {stock}. Cannot update.");
            }

            cartItem.Quantity = newQuantity;
            await _context.SaveChangesAsync();
        }


        public async Task RemoveCartItemAsync(int cartItemId)
        {
            var cartItem = await _context.CartItems.FindAsync(cartItemId);
            if (cartItem != null)
            {
                _context.CartItems.Remove(cartItem);
                await _context.SaveChangesAsync();
            }
        }

        public async Task ClearCartAsync(int userId)
        {
            var cartItems = await _context.CartItems.Where(ci => ci.UserID == userId).ToListAsync();
            if (cartItems.Any())
            {
                _context.CartItems.RemoveRange(cartItems);
                await _context.SaveChangesAsync();
            }
        }
    }
}