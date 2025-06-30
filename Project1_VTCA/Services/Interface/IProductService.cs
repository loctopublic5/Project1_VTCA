using Project1_VTCA.Data;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Project1_VTCA.Services.Interface
{
    public interface IProductService
    {
        IQueryable<Product> GetActiveProductsQuery();
        IQueryable<Product> GetSearchQuery(string searchTerm);
        IQueryable<Product> GetCategoryFilterQuery(List<int> categoryIds);
        Task<(List<Product> Products, int TotalPages)> GetPaginatedProductsAsync(IQueryable<Product> query, int pageNumber, int pageSize, string sortBy);
        Task<Product> GetProductByIdAsync(int productId);
        Task<List<Category>> GetAllProductCategoriesAsync();
    }
}