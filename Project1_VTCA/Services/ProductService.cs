using Microsoft.EntityFrameworkCore;
using Project1_VTCA.Data;
using Project1_VTCA.Services.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Project1_VTCA.Services
{
    public class ProductService : IProductService
    {
        private readonly SneakerShopDbContext _context;
        private readonly IPromotionService _promotionService;

        public ProductService(SneakerShopDbContext context, IPromotionService promotionService)
        {
            _context = context;
            _promotionService = promotionService;
        }

        // Trả về câu truy vấn gốc để có thể xây dựng tiếp
        public IQueryable<Product> GetActiveProductsQuery()
        {
            return _context.Products
                .Include(p => p.ProductCategories)
                .ThenInclude(pc => pc.Category)
                .Where(p => p.IsActive);
        }

        // Áp dụng các bộ lọc và phân trang vào một câu truy vấn có sẵn
        public async Task<(List<Product> Products, int TotalPages)> GetPaginatedProductsAsync(IQueryable<Product> query, int pageNumber, int pageSize, string sortBy)
        {
            switch (sortBy)
            {
                case "price_desc": query = query.OrderByDescending(p => p.Price); break;
                case "price_asc": query = query.OrderBy(p => p.Price); break;
                default: query = query.OrderByDescending(p => p.ProductID); break;
            }

            var totalProducts = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalProducts / (double)pageSize);
            var products = await query.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync();
            return (products, totalPages);
        }

        public IQueryable<Product> GetSearchQuery(string searchTerm)
        {
            var baseQuery = GetActiveProductsQuery();
            var keywords = searchTerm.ToLower().Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            if (!keywords.Any()) return baseQuery;

            foreach (var keyword in keywords)
            {
                baseQuery = baseQuery.Where(p => p.Name.ToLower().Contains(keyword));
            }
            return baseQuery;
        }

        public IQueryable<Product> GetCategoryFilterQuery(List<int> categoryIds)
        {
            var baseQuery = GetActiveProductsQuery();
            if (categoryIds == null || !categoryIds.Any()) return baseQuery;

            return baseQuery.Where(p => p.ProductCategories.Any(pc => categoryIds.Contains(pc.CategoryID)));
        }

        public async Task<List<Category>> GetAllProductCategoriesAsync()
        {
            return await _context.Categories
                .Where(c => c.ParentID != null && c.CategoryType == "Product")
                .OrderBy(c => c.Name).ToListAsync();
        }

        public async Task<Product> GetProductByIdAsync(int productId)
        {
            return await _context.Products
                .Include(p => p.ProductCategories).ThenInclude(pc => pc.Category)
                .Include(p => p.ProductSizes)
                .FirstOrDefaultAsync(p => p.ProductID == productId && p.IsActive);
        }
    }
}