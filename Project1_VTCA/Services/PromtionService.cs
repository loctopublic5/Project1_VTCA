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

            // Lặp qua tất cả các khuyến mãi để tìm ra cái tốt nhất
            foreach (var promo in allPromotions)
            {
                // Kiểm tra xem sản phẩm và người dùng có đủ điều kiện cho khuyến mãi này không
                bool isApplicable = await IsPromotionApplicable(promo, product, user);

                if (isApplicable)
                {
                    decimal currentDiscountAmount = 0;
                    if (promo.DiscountPercentage.HasValue)
                    {
                        currentDiscountAmount = product.Price * (promo.DiscountPercentage.Value / 100);
                    }
                    else if (promo.DiscountAmount.HasValue)
                    {
                        currentDiscountAmount = promo.DiscountAmount.Value;
                    }

                    if (currentDiscountAmount > 0)
                    {
                        decimal currentDiscountedPrice = product.Price - currentDiscountAmount;

                        // Quy tắc mới: Luôn chọn giá sau khi giảm là thấp nhất
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

        // HÀM KIỂM TRA LOGIC ĐÃ ĐƯỢC VIẾT LẠI HOÀN TOÀN
        private async Task<bool> IsPromotionApplicable(Promotion promo, Product product, User user)
        {
            // Tải thông tin danh mục của sản phẩm một lần để tái sử dụng
            var productCategoryIds = await _context.ProductCategories
                                            .Where(pc => pc.ProductID == product.ProductID)
                                            .Select(pc => pc.CategoryID)
                                            .ToListAsync();

            // --- BẮT ĐẦU KIỂM TRA CÁC ĐIỀU KIỆN ---

            // Điều kiện 1: Khuyến mãi cho sản phẩm cụ thể?
            if (promo.ApplicableProductId.HasValue && promo.ApplicableProductId != product.ProductID)
            {
                return false; // Nếu có chỉ định sản phẩm nhưng không khớp -> loại
            }

            // Điều kiện 2: Khuyến mãi cho danh mục cụ thể?
            if (promo.ApplicableCategoryId.HasValue && !productCategoryIds.Contains(promo.ApplicableCategoryId.Value))
            {
                return false; // Nếu có chỉ định danh mục nhưng sản phẩm không thuộc danh mục đó -> loại
            }

            // Điều kiện 3: Khuyến mãi theo giới tính?
            if (!string.IsNullOrEmpty(promo.ApplicableGender) && promo.ApplicableGender != "All")
            {
                // Nếu KM yêu cầu giới tính mà người dùng chưa đăng nhập -> loại
                if (user == null) return false;

                // Nếu giới tính người dùng không khớp với KM -> loại
                if (user.Gender != promo.ApplicableGender) return false;

                // Quy tắc bạn yêu cầu: KM theo giới tính sẽ không áp dụng cho sản phẩm Unisex
                if (product.GenderApplicability == "Unisex") return false;

                // Giới tính sản phẩm phải khớp với giới tính của KM
                if (product.GenderApplicability != promo.ApplicableGender) return false;
            }

            // Nếu vượt qua tất cả các kiểm tra, khuyến mãi có thể được áp dụng
            return true;
        }
    }
}