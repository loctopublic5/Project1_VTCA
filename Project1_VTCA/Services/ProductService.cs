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

        public ProductService(SneakerShopDbContext context)
        {
            _context = context;
        }

        public async Task<(List<Product> Products, int TotalPages)> GetActiveProductsPaginatedAsync(int pageNumber, int pageSize, string sortBy, decimal? minPrice, decimal? maxPrice)
        {
            // 1. Bắt đầu câu truy vấn và INCLUDE tất cả dữ liệu liên quan trước tiên.
            // Chuỗi Include/ThenInclude phải đi liền nhau.
            var query = _context.Products
                .Include(p => p.ProductCategories)
                .ThenInclude(pc => pc.Category)
                .AsQueryable();

            // 2. Áp dụng các bộ lọc WHERE
            query = query.Where(p => p.IsActive);
            if (minPrice.HasValue)
            {
                query = query.Where(p => p.Price >= minPrice.Value);
            }
            if (maxPrice.HasValue)
            {
                query = query.Where(p => p.Price <= maxPrice.Value);
            }

            // 3. Đếm tổng số sản phẩm SAU KHI lọc (để phân trang cho đúng)
            var totalProducts = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalProducts / (double)pageSize);

            // 4. Áp dụng SẮP XẾP
            switch (sortBy)
            {
                case "price_desc":
                    query = query.OrderByDescending(p => p.Price);
                    break;
                case "price_asc":
                    query = query.OrderBy(p => p.Price);
                    break;
                default:
                    query = query.OrderByDescending(p => p.ProductID);
                    break;
            }

            // 5. Áp dụng PHÂN TRANG và thực thi câu truy vấn
            var products = await query
                                 .Skip((pageNumber - 1) * pageSize)
                                 .Take(pageSize)
                                 .ToListAsync();

            return (products, totalPages);
        }

        // ... Các phương thức còn lại giữ nguyên ...
        public async Task<List<Product>> SearchProductsAsync(string searchTerm)
        {
            var keywords = searchTerm.ToLower().Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            if (!keywords.Any())
            {
                return new List<Product>();
            }

            var query = _context.Products
                .Where(p => p.IsActive)
                .Include(p => p.ProductCategories)
                .ThenInclude(pc => pc.Category)
                .AsQueryable();

            foreach (var keyword in keywords)
            {
                query = query.Where(p => p.Name.ToLower().Contains(keyword));
            }

            return await query.ToListAsync();
        }

        public async Task<List<Category>> GetAllProductCategoriesAsync()
        {
            return await _context.Categories
                .Where(c => c.ParentID != null && c.CategoryType == "Product")
                .OrderBy(c => c.Name)
                .ToListAsync();
        }



        public async Task<(List<Product> Products, int TotalPages)> GetProductsByCategoriesPaginatedAsync(List<int> categoryIds, int pageNumber, int pageSize)
        {
            if (categoryIds == null || !categoryIds.Any())
            {
                return (new List<Product>(), 0);
            }

            var query = _context.Products
                .Include(p => p.ProductCategories)
                .ThenInclude(pc => pc.Category)
                .Where(p => p.IsActive && p.ProductCategories.Any(pc => categoryIds.Contains(pc.CategoryID)))
                .OrderBy(p => p.Price); // Sắp xếp theo giá tăng dần theo yêu cầu

            var totalProducts = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalProducts / (double)pageSize);

            var products = await query
                                 .Skip((pageNumber - 1) * pageSize)
                                 .Take(pageSize)
                                 .ToListAsync();

            return (products, totalPages);
        }
    }
}