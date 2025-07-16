using Microsoft.EntityFrameworkCore;
using Project1_VTCA.Data;
using Project1_VTCA.Services.Interface;
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

  
            if (promo.ApplicableProductId.HasValue && promo.ApplicableProductId != product.ProductID)
                return false;

            if (promo.ApplicableCategoryId.HasValue && !productCategoryIds.Contains(promo.ApplicableCategoryId.Value))
                return false;

  
            if (!string.IsNullOrEmpty(promo.ApplicableGender))
            {
             
                if (promo.ApplicableGender == "All")
                {
                   
                }
                
                else
                {
                    
                    if (user == null || user.Gender != promo.ApplicableGender)
                    {
                        return false;
                    }
                   
                    if (product.GenderApplicability != promo.ApplicableGender)
                    {
                        return false;
                    }
                }
            }

    
            return true;
        }
    }
}