using Microsoft.EntityFrameworkCore;
using Project1_VTCA.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Project1_VTCA.Services
{
    public class ProductService : IProductService
    {
        private readonly SneakerShopDbContext _context;
        private readonly IPromotionService _promotionService; // Thêm service này

        // Sửa lại hàm dựng để nhận IPromotionService
        public ProductService(SneakerShopDbContext context, IPromotionService promotionService)
        {
            _context = context;
            _promotionService = promotionService;
        }

        public async Task<(List<Product> Products, int TotalPages)> GetActiveProductsPaginatedAsync(int pageNumber, int pageSize, string sortBy, decimal? minPrice, decimal? maxPrice)
        {
            var query = _context.Products
                .Include(p => p.ProductCategories).ThenInclude(pc => pc.Category)
                .Where(p => p.IsActive);

            if (minPrice.HasValue) query = query.Where(p => p.Price >= minPrice.Value);
            if (maxPrice.HasValue) query = query.Where(p => p.Price <= maxPrice.Value);

            var totalProducts = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalProducts / (double)pageSize);

            switch (sortBy)
            {
                case "price_desc": query = query.OrderByDescending(p => p.Price); break;
                case "price_asc": query = query.OrderBy(p => p.Price); break;
                default: query = query.OrderByDescending(p => p.ProductID); break;
            }

            var products = await query.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync();
            return (products, totalPages);
        }

        public async Task<(List<Product> Products, int TotalPages)> SearchProductsAsync(string searchTerm, int pageNumber, int pageSize)
        {
            var keywords = searchTerm.ToLower().Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            var query = _context.Products.Include(p => p.ProductCategories).ThenInclude(pc => pc.Category).Where(p => p.IsActive);
            if (keywords.Any())
            {
                foreach (var keyword in keywords)
                {
                    query = query.Where(p => p.Name.ToLower().Contains(keyword));
                }
            }
            var totalProducts = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalProducts / (double)pageSize);
            var products = await query.OrderByDescending(p => p.ProductID).Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync();
            return (products, totalPages);
        }

        public async Task<List<Category>> GetAllProductCategoriesAsync()
        {
            return await _context.Categories.Where(c => c.ParentID != null && c.CategoryType == "Product").OrderBy(c => c.Name).ToListAsync();
        }

        public async Task<(List<Product> Products, int TotalPages)> GetProductsByCategoriesPaginatedAsync(List<int> categoryIds, int pageNumber, int pageSize)
        {
            var query = _context.Products.Include(p => p.ProductCategories).ThenInclude(pc => pc.Category).Where(p => p.IsActive && p.ProductCategories.Any(pc => categoryIds.Contains(pc.CategoryID)));
            var totalProducts = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalProducts / (double)pageSize);
            var products = await query.OrderBy(p => p.Price).Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync();
            return (products, totalPages);
        }

     

        public async Task<Product> GetProductByIdAsync(int productId)
        {
            return await _context.Products
                .Include(p => p.ProductCategories)
                .ThenInclude(pc => pc.Category)
                .Include(p => p.ProductSizes)
                .FirstOrDefaultAsync(p => p.ProductID == productId && p.IsActive);
        }
    }
}