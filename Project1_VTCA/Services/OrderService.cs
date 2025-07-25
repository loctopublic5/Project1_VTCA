﻿using Microsoft.EntityFrameworkCore;
using Project1_VTCA.Data;
using Project1_VTCA.DTOs;
using Project1_VTCA.Services.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace Project1_VTCA.Services
{
    public class OrderService : IOrderService
    {
        private readonly SneakerShopDbContext _context;
        private readonly IPromotionService _promotionService;
        private readonly IProductService _productService;

        public OrderService(SneakerShopDbContext context, IPromotionService promotionService, IProductService productService)
        {
            _context = context;
            _promotionService = promotionService;
            _productService = productService;
        }

        #region Customer Order Methods
        public async Task<Order?> GetOrderByIdAsync(int orderId, int userId)
        {
            var query = _context.Orders
                .Include(o => o.User)
                .Include(o => o.OrderDetails)
                .ThenInclude(od => od.Product)
                .AsQueryable();

            
            if (userId != 0)
            {
                query = query.Where(o => o.UserID == userId);
            }

            
            return await query.FirstOrDefaultAsync(o => o.OrderID == orderId);
        }

        public async Task<BalanceUpdateResult> RequestCancellationAsync(int userId, int orderId, string reason)
        {

            var user = await _context.Users.FindAsync(userId);
            if (user == null)
            {
                return new BalanceUpdateResult(false, "Không tìm thấy người dùng.", 0);
            }
            var initialBalance = user.Balance;

            var strategy = _context.Database.CreateExecutionStrategy();
            return await strategy.ExecuteAsync(async () =>
            {
                using var transaction = await _context.Database.BeginTransactionAsync();
                try
                {
                    var order = await _context.Orders
                        .Include(o => o.User)
                        .FirstOrDefaultAsync(o => o.OrderID == orderId && o.UserID == userId);


                    if (order == null)
                    {
                        await transaction.RollbackAsync();
                        return new BalanceUpdateResult(false, "Không tìm thấy đơn hàng.", initialBalance);
                    }
                    if (order.Status != "PendingAdminApproval")
                    {
                        await transaction.RollbackAsync();
                        return new BalanceUpdateResult(false, "Chỉ có thể hủy đơn hàng đang chờ xác nhận.", initialBalance);
                    }

                    if (order.PaymentMethod == "Thanh toán ngay (trừ vào số dư)" && order.User != null)
                    {
                        order.User.Balance += order.TotalPrice;
                    }

                    order.Status = "CustomerCancelled";
                    order.CustomerCancellationReason = reason;

                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();


                    return new BalanceUpdateResult(true, "Đã hủy đơn hàng thành công và hoàn tiền (nếu có).", order.User?.Balance ?? initialBalance);
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();

                    return new BalanceUpdateResult(false, $"Lỗi hệ thống khi hủy đơn hàng: {ex.Message}", initialBalance);
                }
            });
        }


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
                    var user = await _context.Users.FindAsync(userId);
                    if (user == null) throw new InvalidOperationException("Không tìm thấy người dùng.");

                    decimal subTotal = 0;
                    int totalQuantity = 0;
                    foreach (var item in items)
                    {
                        var (discountedPrice, _) = await _promotionService.CalculateDiscountedPriceAsync(item.Product);
                        subTotal += (discountedPrice ?? item.Product.Price) * item.Quantity;
                        totalQuantity += item.Quantity;
                    }

                    var shippingFee = CalculateShippingFee(totalQuantity);
                    var totalPrice = subTotal + shippingFee;


                    if (paymentMethod == "Thanh toán ngay (trừ vào số dư)")
                    {
                        if (user.Balance < totalPrice)
                        {
                            return new ServiceResponse(false, "Số dư không đủ để thực hiện giao dịch.");
                        }

                       
                        user.Balance -= totalPrice;
                    }

                    

                    var order = new Order
                    {
                        UserID = userId,
                        OrderCode = $"SNEAKER{DateTime.Now:yyyyMMddHHmmssfff}",
                        OrderDate = DateTime.Now,
                        Status = "PendingAdminApproval",
                        TotalPrice = totalPrice,
                        ShippingAddress = shippingAddress,
                        ShippingPhone = shippingPhone,
                        PaymentMethod = paymentMethod
                    };
                    _context.Orders.Add(order);
                    await _context.SaveChangesAsync();

                    foreach (var item in items)
                    {
                        var (discountedPrice, _) = await _promotionService.CalculateDiscountedPriceAsync(item.Product);
                        var orderDetail = new OrderDetail
                        {
                            OrderID = order.OrderID,
                            ProductID = item.ProductID,
                            Size = item.Size,
                            Quantity = item.Quantity,
                            UnitPrice = discountedPrice ?? item.Product.Price
                        };
                        _context.OrderDetails.Add(orderDetail);
                    }



                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();

                    return new ServiceResponse(true, order.OrderID.ToString());
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    return new ServiceResponse(false, $"Lỗi hệ thống khi tạo đơn hàng: {ex.Message}");
                }
            });
        }

        
        public async Task<ServiceResponse> ConfirmOrderAsync(int orderId, int adminId)
        {
            var strategy = _context.Database.CreateExecutionStrategy();
            return await strategy.ExecuteAsync(async () =>
            {
                using var transaction = await _context.Database.BeginTransactionAsync();
                try
                {
                    var order = await _context.Orders
                        .Include(o => o.User)
                        .Include(o => o.OrderDetails).ThenInclude(od => od.Product)
                        .FirstOrDefaultAsync(o => o.OrderID == orderId);

                    if (order == null) throw new InvalidOperationException("Không tìm thấy đơn hàng.");
                    if (order.Status != "PendingAdminApproval") return new ServiceResponse(false, "Đơn hàng đã được xử lý trước đó.");

        
                    foreach (var detail in order.OrderDetails)
                    {
                        var productSize = await _context.ProductSizes.FirstOrDefaultAsync(ps => ps.ProductID == detail.ProductID && ps.Size == detail.Size);
                        if (productSize == null || (productSize.QuantityInStock ?? 0) < detail.Quantity)
                        {
                            throw new InvalidOperationException($"Không đủ tồn kho cho sản phẩm '{detail.Product.Name}' - Size {detail.Size}.");
                        }
                    }

                 
                    foreach (var detail in order.OrderDetails)
                    {

                        var productSize = await _context.ProductSizes.FirstAsync(ps => ps.ProductID == detail.ProductID && ps.Size == detail.Size);
                        productSize.QuantityInStock -= detail.Quantity;

                 
                        var product = await _context.Products.FirstAsync(p => p.ProductID == detail.ProductID);
                        product.TotalQuantity -= detail.Quantity;
                    }

                    order.Status = "Processing";
                    order.ApprovedByAdminID = adminId;
                    decimal subTotal = order.OrderDetails.Sum(od => od.UnitPrice * od.Quantity);
                    order.User.TotalSpending += subTotal;

                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();

                    return new ServiceResponse(true, "Đã xác nhận và trừ kho thành công.");
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    return new ServiceResponse(false, $"Lỗi hệ thống khi xác nhận đơn hàng: {ex.Message}");
                }
            });
        }

        public async Task<(List<Order> Orders, int TotalPages)> GetOrdersAsync(int userId, string? statusFilter, int pageNumber, int pageSize)
        {
            var query = _context.Orders
                .Where(o => o.UserID == userId)
                .OrderByDescending(o => o.OrderDate)
                .AsQueryable();

            if (!string.IsNullOrEmpty(statusFilter))
            {
        
                if (statusFilter.Contains('|'))
                {
                    var statuses = statusFilter.Split('|', StringSplitOptions.RemoveEmptyEntries);
                    query = query.Where(o => statuses.Contains(o.Status));
                }
                else 
                {
                    query = query.Where(o => o.Status == statusFilter);
                }
            }

            var totalOrders = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalOrders / (double)pageSize);

            var orders = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (orders, totalPages);
        }




        #endregion


        #region Admin Methods

        #region Admin Methods 2



        // --- PHƯƠNG THỨC MỚI: XỬ LÝ TỪNG PHẦN VÀ KIỂM TRA TỒN KHO ---
        public async Task<ServiceResponse> AttemptToConfirmOrderAsync(int orderId, int adminId)
        {
            var strategy = _context.Database.CreateExecutionStrategy();
            return await strategy.ExecuteAsync(async () =>
            {
                using var transaction = await _context.Database.BeginTransactionAsync();
                try
                {
                    var order = await _context.Orders
                        .Include(o => o.User)
                        .Include(o => o.OrderDetails).ThenInclude(od => od.Product)
                        .FirstOrDefaultAsync(o => o.OrderID == orderId);

                    if (order == null) throw new InvalidOperationException("Không tìm thấy đơn hàng.");
                    if (order.Status != "PendingAdminApproval") return new ServiceResponse(false, "Đơn hàng đã được xử lý trước đó.");

                    var affectedProductIds = new HashSet<int>();

                    // Bước 1: Trừ kho cho từng sản phẩm trong đơn hàng
                    foreach (var detail in order.OrderDetails)
                    {
                        var productSize = await _context.ProductSizes.FirstOrDefaultAsync(ps => ps.ProductID == detail.ProductID && ps.Size == detail.Size);
                        if (productSize == null || productSize.QuantityInStock < detail.Quantity)
                        {
                            throw new InvalidOperationException($"Không đủ tồn kho cho sản phẩm '{detail.Product.Name}' - Size {detail.Size}.");
                        }

                        // Chỉ trừ kho, không cần gọi service khác
                        productSize.QuantityInStock -= detail.Quantity;
                    }

                    // Bước 2: Cập nhật trạng thái đơn hàng và thông tin khách hàng
                    order.Status = "Processing";
                    order.ApprovedByAdminID = adminId;
                    decimal subTotal = order.OrderDetails.Sum(od => od.UnitPrice * od.Quantity);
                    order.User.TotalSpending += subTotal;

                    // Bước 3: Lưu tất cả thay đổi (Order, ProductSize, Product.TotalQuantity) trong một lần
                    await _context.SaveChangesAsync();

                    // Bước 4: Commit transaction
                    await transaction.CommitAsync();

                    return new ServiceResponse(true, "Xác nhận thành công.");
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    return new ServiceResponse(false, $"Lỗi hệ thống khi xác nhận đơn hàng: {ex.Message}");
                }
            });
        }

        // ... (Các phương thức khác giữ nguyên)

        #endregion

        #region Admin Methods 1

        public async Task<ServiceResponse> BulkConfirmOrdersAsync(List<int> orderIds, int adminId)
        {
            if (!orderIds.Any()) return new ServiceResponse(false, "Không có đơn hàng nào được chọn.");

            var strategy = _context.Database.CreateExecutionStrategy();
            return await strategy.ExecuteAsync(async () =>
            {
                using var transaction = await _context.Database.BeginTransactionAsync();
                try
                {
                    var ordersToProcess = await _context.Orders
                        .Include(o => o.User)
                        .Where(o => orderIds.Contains(o.OrderID) && o.Status == "PendingAdminApproval")
                        .ToListAsync();

                    if (ordersToProcess.Count != orderIds.Count)
                    {
                        await transaction.RollbackAsync();
                        return new ServiceResponse(false, "Một vài đơn hàng không hợp lệ hoặc đã được xử lý. Vui lòng thử lại.");
                    }

                    foreach (var order in ordersToProcess)
                    {
                        order.Status = "Processing";
                        order.ApprovedByAdminID = adminId;
                        decimal subTotal = await _context.OrderDetails
                            .Where(od => od.OrderID == order.OrderID)
                            .SumAsync(od => od.UnitPrice * od.Quantity);
                        order.User.TotalSpending += subTotal;
                    }

                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();
                    return new ServiceResponse(true, $"Đã xác nhận thành công {ordersToProcess.Count} đơn hàng.");
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    return new ServiceResponse(false, $"Lỗi hệ thống khi xác nhận hàng loạt: {ex.Message}");
                }
            });
        }

        public async Task<ServiceResponse> BulkRejectOrdersAsync(List<int> orderIds, int adminId, string reason)
        {
            if (!orderIds.Any()) return new ServiceResponse(false, "Không có đơn hàng nào được chọn.");
            if (string.IsNullOrWhiteSpace(reason)) return new ServiceResponse(false, "Lý do hủy không được để trống.");

            var strategy = _context.Database.CreateExecutionStrategy();
            return await strategy.ExecuteAsync(async () =>
            {
                using var transaction = await _context.Database.BeginTransactionAsync();
                try
                {
                    var ordersToReject = await _context.Orders
                        .Include(o => o.User)
                        .Where(o => orderIds.Contains(o.OrderID) && o.Status == "PendingAdminApproval")
                        .ToListAsync();

                    if (ordersToReject.Count != orderIds.Count)
                    {
                        await transaction.RollbackAsync();
                        return new ServiceResponse(false, "Một vài đơn hàng không hợp lệ hoặc đã được xử lý. Vui lòng thử lại.");
                    }

                    foreach (var order in ordersToReject)
                    {
                        // 1. Cập nhật trạng thái và lý do
                        order.Status = "RejectedByAdmin";
                        order.ApprovedByAdminID = adminId;
                        order.AdminDecisionReason = reason;

                        // 2. Hoàn tiền (nếu cần)
                        if (order.PaymentMethod == "Thanh toán ngay (trừ vào số dư)")
                        {
                            order.User.Balance += order.TotalPrice;
                        }

                        // 3. QUAN TRỌNG: KHÔNG HOÀN KHO
                    }

                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();
                    return new ServiceResponse(true, $"Đã hủy thành công {ordersToReject.Count} đơn hàng.");
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    return new ServiceResponse(false, $"Lỗi hệ thống khi hủy hàng loạt: {ex.Message}");
                }
            });
        }



        private async Task<ServiceResponse> ProcessBulkCancellation(List<int> orderIds, int adminId, string finalStatus, string? adminReason)
        {
            var strategy = _context.Database.CreateExecutionStrategy();
            return await strategy.ExecuteAsync(async () =>
            {
                using var transaction = await _context.Database.BeginTransactionAsync();
                try
                {
                    var ordersToCancel = await _context.Orders
                        .Include(o => o.User)
                        .Include(o => o.OrderDetails).ThenInclude(od => od.Product)
                        .Where(o => orderIds.Contains(o.OrderID))
                        .ToListAsync();

                    var validStatuses = new[] { "PendingAdminApproval", "CancellationRequested" };
                    var invalidOrders = ordersToCancel.Where(o => !validStatuses.Contains(o.Status)).ToList();
                    if (invalidOrders.Any())
                    {
                        await transaction.RollbackAsync();
                        return new ServiceResponse(false, $"Các đơn hàng ID: {string.Join(", ", invalidOrders.Select(o => o.OrderID))} không ở trạng thái hợp lệ để xử lý.");
                    }

                    foreach (var order in ordersToCancel)
                    {
                        order.Status = finalStatus;
                        order.ApprovedByAdminID = adminId;
                        if (!string.IsNullOrEmpty(adminReason))
                        {
                            order.AdminDecisionReason = adminReason;
                        }

                        if (order.PaymentMethod == "Thanh toán ngay (trừ vào số dư)")
                        {
                            order.User.Balance += order.TotalPrice;
                        }

                        foreach (var detail in order.OrderDetails)
                        {
                            var productSize = await _context.ProductSizes.FirstOrDefaultAsync(ps => ps.ProductID == detail.ProductID && ps.Size == detail.Size);
                            if (productSize != null)
                            {
                                productSize.QuantityInStock = (productSize.QuantityInStock ?? 0) + detail.Quantity;
                            }
                            detail.Product.TotalQuantity += detail.Quantity;
                        }
                    }

                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();

                    return new ServiceResponse(true, $"Đã xử lý thành công {ordersToCancel.Count} đơn hàng.");
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    return new ServiceResponse(false, $"Lỗi hệ thống khi xử lý hàng loạt: {ex.Message}");
                }
            });
        }

        #endregion

        public async Task<(List<Order> Orders, int TotalPages, int TotalCount)> GetOrdersForAdminAsync(string? statusFilter, int pageNumber, int pageSize)
        {
            var query = _context.Orders
                .Include(o => o.User)
                .OrderByDescending(o => o.OrderDate)
                .AsQueryable();

            if (!string.IsNullOrEmpty(statusFilter))
            {
                if (statusFilter == "ActionRequired")
                {
                    query = query.Where(o => o.Status == "PendingAdminApproval" || o.Status == "CancellationRequested");
                }
                else if (statusFilter.Contains('|'))
                {
                    var statuses = statusFilter.Split('|', StringSplitOptions.RemoveEmptyEntries);
                    query = query.Where(o => statuses.Contains(o.Status));
                }
                else
                {
                    query = query.Where(o => o.Status == statusFilter);
                }
            }

            var totalOrders = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalOrders / (double)pageSize);

            var orders = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (orders, totalPages, totalOrders);
        }

        public async Task<ServiceResponse> ConfirmOrderAsync(int orderId)
        {
            var order = await _context.Orders.Include(o => o.User).FirstOrDefaultAsync(o => o.OrderID == orderId);
            if (order == null) return new ServiceResponse(false, "Không tìm thấy đơn hàng.");
            if (order.Status != "PendingAdminApproval") return new ServiceResponse(false, "Chỉ có thể xác nhận đơn hàng đang ở trạng thái 'Chờ xác nhận'.");

            var strategy = _context.Database.CreateExecutionStrategy();
            return await strategy.ExecuteAsync(async () =>
            {
                using var transaction = await _context.Database.BeginTransactionAsync();
                try
                {
                    order.Status = "Processing";


                    decimal subTotal = await _context.OrderDetails
                        .Where(od => od.OrderID == orderId)
                        .SumAsync(od => od.UnitPrice * od.Quantity);
                    order.User.TotalSpending += subTotal;

                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();

                    return new ServiceResponse(true, "Đã xác nhận đơn hàng thành công.");
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    return new ServiceResponse(false, $"Lỗi hệ thống khi xác nhận đơn hàng: {ex.Message}");
                }
            });
        }


        public async Task<ServiceResponse> RejectOrderAsync(int orderId, int adminId, string reason)
        {
            return await ProcessOrderCancellation(orderId, adminId, "RejectedByAdmin", reason);
        }

     

        private async Task<ServiceResponse> ProcessOrderCancellation(int orderId, int adminId, string finalStatus, string? adminReason)
        {
            var strategy = _context.Database.CreateExecutionStrategy();
            return await strategy.ExecuteAsync(async () =>
            {
                using var transaction = await _context.Database.BeginTransactionAsync();
                try
                {
                    var order = await _context.Orders
                        .Include(o => o.User)
                        .Include(o => o.OrderDetails).ThenInclude(od => od.Product)
                        .FirstOrDefaultAsync(o => o.OrderID == orderId);

                    if (order == null) throw new InvalidOperationException("Không tìm thấy đơn hàng.");

   
                    order.Status = finalStatus;
                    order.ApprovedByAdminID = adminId;
                    if (!string.IsNullOrEmpty(adminReason))
                    {
                        order.AdminDecisionReason = adminReason;
                    }

                    if (order.PaymentMethod == "Thanh toán ngay (trừ vào số dư)")
                    {
                        order.User.Balance += order.TotalPrice;
                    }

         
                    foreach (var detail in order.OrderDetails)
                    {
                        var productSize = await _context.ProductSizes.FirstOrDefaultAsync(ps => ps.ProductID == detail.ProductID && ps.Size == detail.Size);
                        if (productSize != null)
                        {
                            productSize.QuantityInStock = (productSize.QuantityInStock ?? 0) + detail.Quantity;
                        }
                        detail.Product.TotalQuantity += detail.Quantity;
                    }

                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();

                    return new ServiceResponse(true, $"Đơn hàng đã được cập nhật trạng thái thành '{finalStatus}'.");
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    return new ServiceResponse(false, $"Lỗi hệ thống khi xử lý hủy đơn hàng: {ex.Message}");
                }
            });
        }
        #endregion

    }
}