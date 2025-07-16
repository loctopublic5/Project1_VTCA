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

        #region CUSTOMER METHODS
        public IQueryable<Product> GetActiveProductsQuery()
        {
            return _context.Products
                .Include(p => p.ProductCategories)
                .ThenInclude(pc => pc.Category)
                .Include(p => p.ProductSizes) 
                .Where(p => p.IsActive);
        }

        
        public async Task<(List<Product> Products, int TotalPages)> GetInactiveProductsAsync(int pageNumber, int pageSize)
        {
            var query = _context.Products
                .Include(p => p.ProductSizes) 
                .Where(p => !p.IsActive)
                .OrderByDescending(p => p.ProductID);

            var totalProducts = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalProducts / (double)pageSize);
            var products = await query.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync();
            return (products, totalPages);
        }

        // Áp dụng các bộ lọc và phân trang vào một câu truy vấn có sẵn
        public async Task<(List<Product> Products, int TotalPages)> GetPaginatedProductsAsync(IQueryable<Product> query, int pageNumber, int pageSize, string sortBy)
        {
            switch (sortBy)
            {
                case "price_desc": query = query.OrderByDescending(p => p.Price); break;
                case "price_asc": query = query.OrderBy(p => p.Price); break;
                case "stock_desc": 
                    query = query.OrderByDescending(p => p.TotalQuantity) ;
                    break;
                case "stock_asc": 
                    query = query.OrderBy(p => p.TotalQuantity);
                    break;
                default: query = query.OrderByDescending(p => p.ProductID); break;
            }

            var totalProducts = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalProducts / (double)pageSize);
            var products = await query.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync();
            return (products, totalPages);
        }

       

        public string GetDisplayCategory(Product product)
        {
            if (product?.ProductCategories == null || !product.ProductCategories.Any())
            {
                return "N/A";
            }

            var mainCategory = product.ProductCategories
                .Select(pc => pc.Category)
                .FirstOrDefault(c => c.CategoryType == "Product");

            if (mainCategory == null)
            {
                return "N/A";
            }

            // Logic lược bỏ chuỗi "Giày "
            if (mainCategory.Name.StartsWith("Giày "))
            {
                return mainCategory.Name.Substring(5).Trim();
            }

            return mainCategory.Name;
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
        #endregion

        #region ADMIN METHODS

      


        public List<int> GetValidSizesForGender(string gender)
        {
            return gender switch
            {
                "Male" => Enumerable.Range(40, 6).ToList(),    
                "Female" => Enumerable.Range(35, 5).ToList(),  
                "Unisex" => Enumerable.Range(35, 10).ToList(), 
                _ => new List<int>()
            };
        }
        public async Task<Product?> GetProductByIdIncludingInactiveAsync(int productId)
        {
            return await _context.Products
                .Include(p => p.ProductCategories).ThenInclude(pc => pc.Category)
                .Include(p => p.ProductSizes)
     
                .FirstOrDefaultAsync(p => p.ProductID == productId);
        }
        public async Task<Product?> AddNewProductAsync(Product newProduct, List<int> categoryIds)
        {
            var strategy = _context.Database.CreateExecutionStrategy();
            return await strategy.ExecuteAsync(async () =>
            {
                using var transaction = await _context.Database.BeginTransactionAsync();
                try
                {
                   
                    _context.Products.Add(newProduct);
                    await _context.SaveChangesAsync(); 

                   
                    foreach (var categoryId in categoryIds)
                    {
                        var productCategory = new ProductCategory
                        {
                            ProductID = newProduct.ProductID,
                            CategoryID = categoryId
                        };
                        _context.ProductCategories.Add(productCategory);
                    }
                    await _context.SaveChangesAsync();

                    
                    var availableSizes = new List<int>();
                    if (newProduct.GenderApplicability == "Unisex" || newProduct.GenderApplicability == "Male")
                        availableSizes.AddRange(Enumerable.Range(38, 8)); 
                    if (newProduct.GenderApplicability == "Unisex" || newProduct.GenderApplicability == "Female")
                        availableSizes.AddRange(Enumerable.Range(35, 5)); 
                  

                    foreach (var size in availableSizes.Distinct().OrderBy(s => s))
                    {
                        var productSize = new ProductSize
                        {
                            ProductID = newProduct.ProductID,
                            Size = size,
                            QuantityInStock = 0
                        };
                        _context.ProductSizes.Add(productSize);
                    }

                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();
                    return newProduct;
                }
                catch
                {
                    await transaction.RollbackAsync();
                    return null;
                }
            });
        }

        public async Task<ServiceResponse> AddStockAsync(int productId, List<int> sizeIds, int quantityToAdd)
        {
            if (quantityToAdd <= 0) return new ServiceResponse(false, "Số lượng thêm vào phải lớn hơn 0.");
            var sizesToUpdate = await _context.ProductSizes
                .Where(ps => ps.ProductID == productId && sizeIds.Contains(ps.Size))
                .ToListAsync();
            if (!sizesToUpdate.Any()) return new ServiceResponse(false, "Không tìm thấy các size được chọn.");

            foreach (var size in sizesToUpdate)
            {
                size.QuantityInStock += quantityToAdd;
            }
           
            await _context.SaveChangesAsync();
            return new ServiceResponse(true, $"Đã thêm thành công {quantityToAdd} sản phẩm.");
        }

        public async Task<ServiceResponse> UpdateStockAsync(int productId, List<int> sizeIds, int newQuantity)
        {
            if (newQuantity < 0) return new ServiceResponse(false, "Số lượng mới không thể là số âm.");

            var sizesToUpdate = await _context.ProductSizes
                .Where(ps => ps.ProductID == productId && sizeIds.Contains(ps.Size))
                .ToListAsync();

            if (!sizesToUpdate.Any()) return new ServiceResponse(false, "Không tìm thấy các size được chọn.");

            foreach (var size in sizesToUpdate)
            {
                size.QuantityInStock = newQuantity;
            }

            await _context.SaveChangesAsync();
            return new ServiceResponse(true, $"Đã cập nhật tồn kho thành công.");
        }

        public async Task<ServiceResponse> SoftDeleteProductAsync(int productId)
        {
            var product = await _context.Products.FindAsync(productId);
            if (product == null) return new ServiceResponse(false, "Không tìm thấy sản phẩm.");

            product.IsActive = false;
            await _context.SaveChangesAsync();
            return new ServiceResponse(true, "Đã gỡ sản phẩm khỏi kệ thành công.");
        }

        public async Task<List<Category>> GetCategoriesByTypeAsync(string type)
        {
            return await _context.Categories.Where(c => c.CategoryType == type).ToListAsync();
        }

        public async Task<List<ProductSize>> GetProductSizesAsync(int productId)
        {
            return await _context.ProductSizes.Where(ps => ps.ProductID == productId).OrderBy(ps => ps.Size).ToListAsync();
        }

        #endregion
    }
}