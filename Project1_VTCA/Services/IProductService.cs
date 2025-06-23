using Project1_VTCA.Data;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Project1_VTCA.Services
{
    public interface IProductService
    {

        Task<(List<Product> Products, int TotalPages)> GetActiveProductsPaginatedAsync(int pageNumber, int pageSize, string sortBy, decimal? minPrice, decimal? maxPrice);

        Task<List<Product>> SearchProductsAsync(string searchTerm);
        Task<List<Category>> GetAllProductCategoriesAsync();

        // Cập nhật lại phương thức này với đầy đủ tham số
        Task<(List<Product> Products, int TotalPages)> GetProductsByCategoriesPaginatedAsync(List<int> categoryIds, int pageNumber, int pageSize);
    
    }

}