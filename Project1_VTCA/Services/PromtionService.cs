using Microsoft.EntityFrameworkCore;
using Project1_VTCA.Data;
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
                bool isApplicable = IsPromotionApplicable(promo, product, user);

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

        // Hàm helper để kiểm tra điều kiện áp dụng của một khuyến mãi
        private bool IsPromotionApplicable(Promotion promo, Product product, User user)
        {
            // Quy tắc 1: Khuyến mãi theo sản phẩm cụ thể
            if (promo.ApplicableProductId.HasValue && promo.ApplicableProductId != product.ProductID)
            {
                return false;
            }

            // Quy tắc 2: Khuyến mãi theo danh mục cụ thể
            if (promo.ApplicableCategoryId.HasValue)
            {
                var productCategoryIds = product.ProductCategories?.Select(pc => pc.CategoryID).ToList() ?? new System.Collections.Generic.List<int>();
                if (!productCategoryIds.Contains(promo.ApplicableCategoryId.Value))
                {
                    return false;
                }
            }

            // Quy tắc 3: Khuyến mãi theo giới tính (đã được làm chặt chẽ hơn)
            if (!string.IsNullOrEmpty(promo.ApplicableGender))
            {
                // Nếu người dùng chưa đăng nhập, không thể áp dụng KM theo giới tính
                if (user == null) return false;

                // Giới tính của người dùng phải khớp với giới tính của khuyến mãi
                if (user.Gender != promo.ApplicableGender) return false;

                // KHÔNG áp dụng cho sản phẩm của giới tính đối lập
                if (!string.IsNullOrEmpty(promo.ApplicableGender) && promo.ApplicableGender != "All")
                {
                    if (user == null) return false;
                    if (user.Gender != promo.ApplicableGender) return false;
                    if ((user.Gender == "Male" && product.GenderApplicability == "Female") ||
                        (user.Gender == "Female" && product.GenderApplicability == "Male"))
                    {
                        return false;
                    }
                }
            }

            // (Thêm các quy tắc khác như theo size ở đây sau)

            // Nếu vượt qua tất cả các kiểm tra, khuyến mãi có thể được áp dụng
            return true;
        }
    }
}