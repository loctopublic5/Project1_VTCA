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

        public async Task<(List<Product> Products, int TotalPages)> GetActiveProductsPaginatedAsync(int pageNumber, int pageSize)
        {
            var query = _context.Products.Where(p => p.IsActive); //

            var totalProducts = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalProducts / (double)pageSize);

            var products = await query
                                 .Include(p => p.ProductCategories)
                                 .ThenInclude(pc => pc.Category)
                                 .OrderByDescending(p => p.ProductID) //
                                 .Skip((pageNumber - 1) * pageSize)
                                 .Take(pageSize)
                                 .ToListAsync();

            return (products, totalPages);
        }
    }
}