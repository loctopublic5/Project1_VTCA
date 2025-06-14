using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Project1_VTCA.Data;

namespace Project1_VTCA.Services
{
    public class ProductService : IProductService
    {
        // Sử dụng tên DbContext đã cập nhật
        private readonly SneakerShopDbContext _context;

        public ProductService(SneakerShopDbContext context)
        {
            _context = context;
        }
    }
}