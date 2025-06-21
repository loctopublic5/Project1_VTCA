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

    }
}