using Project1_VTCA.Data;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Project1_VTCA.Services
{
    public interface IProductService
    {
        
        Task<(List<Product> Products, int TotalPages)> GetActiveProductsPaginatedAsync(int pageNumber, int pageSize, string sortBy);
        Task<List<Product>> SearchProductsAsync(string searchTerm);
        Task<List<Category>> GetAllProductCategoriesAsync();
        Task<(List<Product> Products, int TotalPages)> GetProductsByCategoriesPaginatedAsync(List<int> categoryIds, int pageNumber, int pageSize);
    }

}