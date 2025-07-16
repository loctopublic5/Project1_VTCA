using Microsoft.EntityFrameworkCore;
using Project1_VTCA.Data;
using Project1_VTCA.Services.Interface;
using System;
using System.Collections;
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

     

        public async Task<ServiceResponse> UpdateCartItemSizeAsync(int userId, int cartItemId, int newSize)
        {
            var sourceItem = await _context.CartItems
                .Include(ci => ci.Product)
                .FirstOrDefaultAsync(ci => ci.CartItemID == cartItemId && ci.UserID == userId);

            if (sourceItem == null)
                return new ServiceResponse(false, "Lỗi: Không tìm thấy sản phẩm trong giỏ hàng.");

            var newProductSize = await _context.ProductSizes
                .AsNoTracking()
                .FirstOrDefaultAsync(ps => ps.ProductID == sourceItem.ProductID && ps.Size == newSize);

            if (newProductSize == null || (newProductSize.QuantityInStock ?? 0) < sourceItem.Quantity)
            {
                return new ServiceResponse(false, $"Lỗi: Size mới không có sẵn hoặc không đủ hàng (Tồn kho: {newProductSize?.QuantityInStock ?? 0}).");
            }

            int stock = newProductSize?.QuantityInStock ?? 0;

            var destinationItem = await _context.CartItems
                .FirstOrDefaultAsync(ci => ci.UserID == userId && ci.ProductID == sourceItem.ProductID && ci.Size == newSize && ci.CartItemID != cartItemId);

            if (destinationItem != null) 
            {
                int mergedQuantity = sourceItem.Quantity + destinationItem.Quantity;
                if (mergedQuantity > stock)
                {
                    return new ServiceResponse(false, $"Lỗi: Không thể gộp. Tổng số lượng sau khi gộp ({mergedQuantity}) sẽ vượt quá tồn kho (còn {stock}).");
                }
                destinationItem.Quantity = mergedQuantity;
                _context.CartItems.Remove(sourceItem);
            }
            else
            {
                sourceItem.Size = newSize;
            }

            await _context.SaveChangesAsync();
            return new ServiceResponse(true, "Cập nhật size thành công!");
        }


    
        #region Other CartService Methods
        public async Task<ServiceResponse> AddToCartAsync(int userId, int productId, int size, int quantity)
        {
            var productSize = await _context.ProductSizes
                .AsNoTracking()
                .FirstOrDefaultAsync(ps => ps.ProductID == productId && ps.Size == size);

            if (productSize == null)
            {
                return new ServiceResponse(false, "Lỗi: Size này không tồn tại cho sản phẩm.");
            }

            int stock = productSize.QuantityInStock ?? 0;

            if (quantity > stock)
            {
                return new ServiceResponse(false, $"Lỗi: Số lượng tồn kho không đủ. Chỉ còn lại {stock}.");
            }

            var existingCartItem = await _context.CartItems
                .FirstOrDefaultAsync(ci => ci.UserID == userId && ci.ProductID == productId && ci.Size == size);

            if (existingCartItem != null)
            {
                int newTotalQuantity = existingCartItem.Quantity + quantity;
                
                if (newTotalQuantity > stock)
                {
                    return new ServiceResponse(false, $"Lỗi: Tổng số lượng trong giỏ và số lượng thêm vào vượt quá tồn kho (còn {stock}).");
                }
                existingCartItem.Quantity = newTotalQuantity;
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
            return new ServiceResponse(true, "Đã thêm/cập nhật sản phẩm trong giỏ hàng thành công!");
        }

        public async Task<List<CartItem>> GetCartItemsAsync(int userId)
        {
            return await _context.CartItems
                .Where(ci => ci.UserID == userId)
                .Include(ci => ci.Product)
                .ThenInclude(p => p.ProductSizes)
                .OrderBy(ci => ci.CartItemID)
                .ToListAsync();
        }

        public async Task<ServiceResponse> UpdateCartItemQuantityAsync(int cartItemId, int newQuantity)
        {
            var cartItem = await _context.CartItems
                .Include(ci => ci.Product)
                .ThenInclude(p => p.ProductSizes)
                .FirstOrDefaultAsync(ci => ci.CartItemID == cartItemId);

            if (cartItem == null) return new ServiceResponse(false, "Lỗi: Không tìm thấy sản phẩm trong giỏ hàng.");
            if (newQuantity <= 0) return new ServiceResponse(false, "Lỗi: Số lượng mới phải lớn hơn 0.");
            

            var stock = cartItem.Product.ProductSizes.FirstOrDefault(ps => ps.Size == cartItem.Size)?.QuantityInStock ?? 0;
            if (newQuantity > stock) return new ServiceResponse(false, $"Lỗi: Số lượng tồn kho không đủ (chỉ còn {stock}).");

            cartItem.Quantity = newQuantity;
            await _context.SaveChangesAsync();
            return new ServiceResponse(true, "Cập nhật số lượng thành công!");
        }

        public async Task RemoveCartItemsAsync(int userId, List<int> cartItemIds)
        {
            var itemsToRemove = await _context.CartItems
                .Where(ci => ci.UserID == userId && cartItemIds.Contains(ci.CartItemID))
                .ToListAsync();

            if (itemsToRemove.Any())
            {
                _context.CartItems.RemoveRange(itemsToRemove);
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
        #endregion
    }
}