using Microsoft.EntityFrameworkCore;
using Project1_VTCA.Data;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Project1_VTCA.Services
{
    public class PromotionService : IPromotionService
    {
        private readonly SneakerShopDbContext _context;
        private readonly ISessionService _sessionService;

        public PromotionService(SneakerShopDbContext context, ISessionService sessionService)
        {
            _context = context;
            _sessionService = sessionService;
        }

        public async Task<(decimal? DiscountedPrice, string PromotionCode)> CalculateDiscountedPriceAsync(Product product)
        {
            if (product == null) return (null, null);

            var user = _sessionService.CurrentUser;

            var allPromotions = await _context.Promotions
                .Where(p => p.IsActive && p.ExpiryDate > System.DateTime.Now)
                .ToListAsync();

            decimal? bestDiscountedPrice = null;
            string bestPromotionCode = null;

            foreach (var promo in allPromotions)
            {
                bool isApplicable = await IsPromotionApplicable(promo, product, user);

                if (isApplicable)
                {
                    decimal currentDiscountAmount = promo.DiscountPercentage.HasValue
                        ? product.Price * (promo.DiscountPercentage.Value / 100)
                        : promo.DiscountAmount ?? 0;

                    if (currentDiscountAmount > 0)
                    {
                        decimal currentDiscountedPrice = product.Price - currentDiscountAmount;
                        if (!bestDiscountedPrice.HasValue || currentDiscountedPrice < bestDiscountedPrice.Value)
                        {
                            bestDiscountedPrice = currentDiscountedPrice;
                            bestPromotionCode = promo.Code;
                        }
                    }
                }
            }
            return (bestDiscountedPrice, bestPromotionCode);
        }

        // --- HÀM KIỂM TRA LOGIC ĐÃ ĐƯỢC CẬP NHẬT CHẶT CHẼ ---
        private async Task<bool> IsPromotionApplicable(Promotion promo, Product product, User user)
        {
            // Tải thông tin danh mục của sản phẩm một lần để tái sử dụng
            var productCategoryIds = await _context.ProductCategories
                                            .Where(pc => pc.ProductID == product.ProductID)
                                            .Select(pc => pc.CategoryID)
                                            .ToListAsync();

            // Điều kiện 1: Khuyến mãi cho sản phẩm cụ thể?
            if (promo.ApplicableProductId.HasValue && promo.ApplicableProductId != product.ProductID)
                return false;

            // Điều kiện 2: Khuyến mãi cho danh mục cụ thể?
            if (promo.ApplicableCategoryId.HasValue && !productCategoryIds.Contains(promo.ApplicableCategoryId.Value))
                return false;

            // Điều kiện 3: Khuyến mãi theo giới tính (LOGIC MỚI)
            if (!string.IsNullOrEmpty(promo.ApplicableGender))
            {
                // Nếu KM dành cho TẤT CẢ thì bỏ qua các kiểm tra giới tính khác
                if (promo.ApplicableGender == "All")
                {
                    // Không cần làm gì thêm, tiếp tục kiểm tra các điều kiện khác (nếu có)
                }
                // Nếu KM dành cho giới tính cụ thể (Male/Female)
                else
                {
                    // Người dùng phải đăng nhập VÀ có giới tính khớp với KM
                    if (user == null || user.Gender != promo.ApplicableGender)
                    {
                        return false;
                    }
                    // Giới tính SẢN PHẨM cũng phải khớp chính xác với giới tính của KM
                    if (product.GenderApplicability != promo.ApplicableGender)
                    {
                        return false;
                    }
                }
            }

            // Nếu vượt qua tất cả các kiểm tra, khuyến mãi có thể được áp dụng
            return true;
        }
    }
}