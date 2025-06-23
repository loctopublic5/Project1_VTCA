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

        public async Task<(List<Product> Products, int TotalPages)> GetActiveProductsPaginatedAsync(int pageNumber, int pageSize, string sortBy)
        {
            var query = _context.Products
                .Where(p => p.IsActive)
                .Include(p => p.ProductCategories)
                .ThenInclude(pc => pc.Category)
                .AsQueryable(); // Ensure the query is treated as IQueryable<Product>

            // Xử lý logic sắp xếp
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

            var totalProducts = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalProducts / (double)pageSize);

            var products = await query
                                 .Skip((pageNumber - 1) * pageSize)
                                 .Take(pageSize)
                                 .ToListAsync();

            return (products, totalPages);
        }
        public async Task<List<Product>> SearchProductsAsync(string searchTerm)
        {
            // Tách chuỗi tìm kiếm thành các từ khóa, loại bỏ các khoảng trắng thừa
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

            // Lọc các sản phẩm có tên chứa TẤT CẢ các từ khóa
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