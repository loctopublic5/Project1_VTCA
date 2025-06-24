using Project1_VTCA.Data;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Project1_VTCA.Services
{
    public interface IProductService
    {
        Task<(List<Product> Products, int TotalPages)> GetActiveProductsPaginatedAsync(int pageNumber, int pageSize, string sortBy, decimal? minPrice, decimal? maxPrice);
        Task<(List<Product> Products, int TotalPages)> SearchProductsAsync(string searchTerm, int pageNumber, int pageSize);
        Task<List<Category>> GetAllProductCategoriesAsync();
        Task<(List<Product> Products, int TotalPages)> GetProductsByCategoriesPaginatedAsync(List<int> categoryIds, int pageNumber, int pageSize);
     
        Task<Product> GetProductByIdAsync(int productId);
    }
}