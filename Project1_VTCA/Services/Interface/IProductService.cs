using Project1_VTCA.Data;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Project1_VTCA.Services.Interface
{
    public interface IProductService
    {
        /// CUSTOMER METHODS
        IQueryable<Product> GetActiveProductsQuery();
        IQueryable<Product> GetSearchQuery(string searchTerm);
        IQueryable<Product> GetCategoryFilterQuery(List<int> categoryIds);
        Task<(List<Product> Products, int TotalPages)> GetPaginatedProductsAsync(IQueryable<Product> query, int pageNumber, int pageSize, string sortBy);
        Task<Product> GetProductByIdAsync(int productId);
        Task<List<Category>> GetAllProductCategoriesAsync();
        string GetDisplayCategory(Product product);

        /// ADMIN METHODS
        Task<ServiceResponse> AddStockAsync(int productId, List<int> sizeIds, int quantityToAdd);
        Task<ServiceResponse> UpdateStockAsync(int productId, List<int> sizeIds, int newQuantity);
        Task<ServiceResponse> SoftDeleteProductAsync(int productId);
        Task<Product?> AddNewProductAsync(Product newProduct, List<int> categoryIds);
        Task<(List<Product> Products, int TotalPages)> GetInactiveProductsAsync(int pageNumber, int pageSize);
        Task<List<Category>> GetCategoriesByTypeAsync(string type);
        Task<List<ProductSize>> GetProductSizesAsync(int productId);
        Task<Product?> GetProductByIdIncludingInactiveAsync(int productId);
        List<int> GetValidSizesForGender(string gender);
    }
}