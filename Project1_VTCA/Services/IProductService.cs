using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Project1_VTCA.Data;

namespace Project1_VTCA.Services
{
    public interface IProductService
    {
        Task<(List<Product> Products, int TotalPages)> GetActiveProductsPaginatedAsync(int pageNumber, int pageSize);
    }
}
