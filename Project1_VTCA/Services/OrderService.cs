using Microsoft.EntityFrameworkCore;
using Project1_VTCA.Data;
using Project1_VTCA.Services.Interface;
using Project1_VTCA.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Project1_VTCA.Services
{
    public class OrderService : IOrderService
    {
        private readonly SneakerShopDbContext _context;
        private readonly IPromotionService _promotionService;

        public OrderService(SneakerShopDbContext context, IPromotionService promotionService)
        {
            _context = context;
            _promotionService = promotionService;
        }

        public decimal CalculateShippingFee(int totalQuantity)
        {
            if (totalQuantity <= 5) return 20000;
            if (totalQuantity <= 15) return 50000;
            return 70000;
        }

        public async Task<ServiceResponse> CreateOrderAsync(int userId, List<CartItem> items, string shippingAddress, string paymentMethod)
        {
            if (!items.Any())
            {
                return new ServiceResponse(false, "Không có sản phẩm nào để tạo đơn hàng.");
            }

            // SỬA LỖI: Áp dụng Execution Strategy để quản lý transaction và retry
            var strategy = _context.Database.CreateExecutionStrategy();

            return await strategy.ExecuteAsync(async () =>
            {
                // Toàn bộ logic được bọc trong transaction bên trong strategy
                using var transaction = await _context.Database.BeginTransactionAsync();
                try
                {
                    decimal subTotal = 0;
                    int totalQuantity = 0;

                    foreach (var item in items)
                    {
                        var product = await _context.Products.FindAsync(item.ProductID);
                        if (product == null) throw new InvalidOperationException($"Sản phẩm với ID {item.ProductID} không tồn tại.");

                        var (discountedPrice, _) = await _promotionService.CalculateDiscountedPriceAsync(product);
                        subTotal += (discountedPrice ?? product.Price) * item.Quantity;
                        totalQuantity += item.Quantity;
                    }

                    var shippingFee = CalculateShippingFee(totalQuantity);
                    var totalPrice = subTotal + shippingFee;

                    var user = await _context.Users.FindAsync(userId);
                    if (paymentMethod == "Thanh toán ngay (trừ vào số dư)" && user.Balance < totalPrice)
                    {
                        // Không cần rollback ở đây vì ExecuteAsync sẽ xử lý
                        return new ServiceResponse(false, "Số dư không đủ để thực hiện giao dịch.");
                    }

                    if (paymentMethod == "Thanh toán ngay (trừ vào số dư)")
                    {
                        user.Balance -= totalPrice;
                    }

                    var order = new Order
                    {
                        UserID = userId,
                        OrderCode = $"SNEAKER{DateTime.Now:yyyyMMddHHmmss}",
                        OrderDate = DateTime.Now,
                        Status = "PendingAdminApproval",
                        TotalPrice = totalPrice,
                        ShippingAddress = shippingAddress,
                        ShippingPhone = user.PhoneNumber,
                        PaymentMethod = paymentMethod
                    };
                    _context.Orders.Add(order);
                    await _context.SaveChangesAsync();

                    foreach (var item in items)
                    {
                        var product = await _context.Products.FindAsync(item.ProductID);
                        var (discountedPrice, _) = await _promotionService.CalculateDiscountedPriceAsync(product);

                        var orderDetail = new OrderDetail
                        {
                            OrderID = order.OrderID,
                            ProductID = item.ProductID,
                            Size = item.Size,
                            Quantity = item.Quantity,
                            UnitPrice = discountedPrice ?? product.Price
                        };
                        _context.OrderDetails.Add(orderDetail);

                        var productSize = await _context.ProductSizes.FirstOrDefaultAsync(ps => ps.ProductID == item.ProductID && ps.Size == item.Size);
                        if (productSize == null || (productSize.QuantityInStock ?? 0) < item.Quantity)
                        {
                            // Ném exception để strategy bắt và rollback
                            throw new InvalidOperationException($"Không đủ hàng cho sản phẩm {product.Name} - Size {item.Size}.");
                        }
                        productSize.QuantityInStock -= item.Quantity;
                    }

                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();

                    return new ServiceResponse(true, order.OrderCode);
                }
                catch (Exception ex)
                {
                    // transaction sẽ tự động được rollback khi using block kết thúc hoặc khi strategy retry
                    return new ServiceResponse(false, $"Đã xảy ra lỗi không mong muốn khi tạo đơn hàng: {ex.Message}");
                }
            });
        }
    }
}