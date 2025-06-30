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

        // --- HIỆN THỰC CÁC PHƯƠNG THỨC MỚI ---
        public async Task<Order?> GetOrderByIdAsync(int orderId, int userId)
        {
            return await _context.Orders
                .Include(o => o.OrderDetails)
                .ThenInclude(od => od.Product)
                .FirstOrDefaultAsync(o => o.OrderID == orderId && o.UserID == userId);
        }

        public async Task<ServiceResponse> RequestCancellationAsync(int userId, int orderId, string reason)
        {
            var order = await _context.Orders.FirstOrDefaultAsync(o => o.OrderID == orderId && o.UserID == userId);
            if (order == null)
            {
                return new ServiceResponse(false, "Không tìm thấy đơn hàng với ID đã nhập.");
            }

            if (order.Status != "PendingAdminApproval")
            {
                return new ServiceResponse(false, "Chỉ có thể yêu cầu hủy các đơn hàng đang ở trạng thái 'Chờ xác nhận'.");
            }

            order.Status = "CancellationRequested";
            order.CustomerCancellationReason = reason;
            await _context.SaveChangesAsync();

            return new ServiceResponse(true, "Yêu cầu hủy đơn hàng đã được gửi thành công.");
        }

        // ... Các phương thức khác giữ nguyên
        #region Other OrderService Methods
        public decimal CalculateShippingFee(int totalQuantity)
        {
            if (totalQuantity <= 5) return 20000;
            if (totalQuantity <= 15) return 50000;
            return 70000;
        }

        public async Task<ServiceResponse> CreateOrderAsync(int userId, List<CartItem> items, string shippingAddress, string shippingPhone, string paymentMethod)
        {
            var strategy = _context.Database.CreateExecutionStrategy();
            return await strategy.ExecuteAsync(async () =>
            {
                using var transaction = await _context.Database.BeginTransactionAsync();
                try
                {
                    var now = DateTime.Now;
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
                        return new ServiceResponse(false, "Số dư không đủ để thực hiện giao dịch.");
                    }
                    if (paymentMethod == "Thanh toán ngay (trừ vào số dư)")
                    {
                        user.Balance -= totalPrice;
                    }
                    var order = new Order
                    {
                        UserID = userId,
                        OrderCode = $"SNEAKER{now:yyyyMMddHHmmssfff}",
                        OrderDate = now,
                        Status = "PendingAdminApproval",
                        TotalPrice = totalPrice,
                        ShippingAddress = shippingAddress,
                        ShippingPhone = shippingPhone,
                        PaymentMethod = paymentMethod,
                        AdminDecisionReason = null,
                        CustomerCancellationReason = null
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
                            throw new InvalidOperationException($"Không đủ hàng cho sản phẩm {product.Name} - Size {item.Size}.");
                        }
                        productSize.QuantityInStock -= item.Quantity;
                        var productToUpdate = await _context.Products.FindAsync(item.ProductID);
                        if (productToUpdate != null)
                        {
                            productToUpdate.TotalQuantity -= item.Quantity;
                        }
                    }
                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();
                    return new ServiceResponse(true, order.OrderCode);
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    return new ServiceResponse(false, $"Lỗi hệ thống khi tạo đơn hàng: {ex.Message}");
                }
            });
        }

        public async Task<(List<Order> Orders, int TotalPages)> GetOrdersAsync(int userId, string? statusFilter, int pageNumber, int pageSize)
        {
            var query = _context.Orders
                .Where(o => o.UserID == userId)
                .OrderByDescending(o => o.OrderDate);

            if (!string.IsNullOrEmpty(statusFilter))
            {
                query = (IOrderedQueryable<Order>)query.Where(o => o.Status == statusFilter);
            }

            var totalOrders = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalOrders / (double)pageSize);

            var orders = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (orders, totalPages);
        }

        public async Task<ServiceResponse> ApproveCancellationAsync(int orderId)
        {
            var order = await _context.Orders.Include(o => o.User).FirstOrDefaultAsync(o => o.OrderID == orderId);
            if (order == null) return new ServiceResponse(false, "Không tìm thấy đơn hàng.");
            if (order.PaymentMethod == "Thanh toán ngay (trừ vào số dư)")
            {
                order.User.Balance += order.TotalPrice;
            }
            order.Status = "Cancelled";
            await _context.SaveChangesAsync();
            return new ServiceResponse(true, "Đã phê duyệt hủy đơn hàng và hoàn tiền (nếu có).");
        }

        public async Task<ServiceResponse> ConfirmOrderAsync(int orderId)
        {
            var order = await _context.Orders.Include(o => o.User).FirstOrDefaultAsync(o => o.OrderID == orderId);
            if (order == null) return new ServiceResponse(false, "Không tìm thấy đơn hàng.");
            decimal subTotal = await _context.OrderDetails.Where(od => od.OrderID == orderId).SumAsync(od => od.UnitPrice * od.Quantity);
            order.User.TotalSpending += subTotal;
            order.Status = "Processing";
            await _context.SaveChangesAsync();
            return new ServiceResponse(true, "Đã xác nhận đơn hàng thành công.");
        }
        #endregion
    }
}