using Microsoft.EntityFrameworkCore;
using Project1_VTCA.Data;
using Project1_VTCA.Services.Interface;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Project1_VTCA.Services
{
    public class AddressService : IAddressService
    {
        private readonly SneakerShopDbContext _context;

        public AddressService(SneakerShopDbContext context)
        {
            _context = context;
        }

        public async Task<ServiceResponse> SoftDeleteAddressAsync(int addressId, int userId)
        {
            var address = await _context.Addresses.FirstOrDefaultAsync(a => a.AddressID == addressId && a.UserID == userId);
            if (address == null) return new ServiceResponse(false, "Không tìm thấy địa chỉ.");

            // NÂNG CẤP: Không cho phép xóa địa chỉ đang là mặc định.
            if (address.IsDefault)
            {
                return new ServiceResponse(false, "Lỗi: Không thể xóa địa chỉ mặc định. Vui lòng đặt một địa chỉ khác làm mặc định trước.");
            }

            address.IsActive = false;
            await _context.SaveChangesAsync();
            return new ServiceResponse(true, "Xóa địa chỉ thành công.");
        }

        // ... các phương thức khác của AddressService không đổi
        #region Other AddressService Methods
        public async Task<List<Address>> GetActiveAddressesAsync(int userId)
        {
            return await _context.Addresses
                .Where(a => a.UserID == userId && a.IsActive == true)
                .OrderByDescending(a => a.IsDefault)
                .ToListAsync();
        }

        public async Task<ServiceResponse> AddAddressAsync(Address newAddress)
        {
            var userAddresses = await GetActiveAddressesAsync(newAddress.UserID);

            if (!userAddresses.Any() && newAddress.IsActive)
            {
                newAddress.IsDefault = true;
            }
            else if (newAddress.IsDefault)
            {
                var currentDefault = userAddresses.FirstOrDefault(a => a.IsDefault);
                if (currentDefault != null)
                {
                    currentDefault.IsDefault = false;
                }
            }

            _context.Addresses.Add(newAddress);
            await _context.SaveChangesAsync();
            return new ServiceResponse(true, "Thêm địa chỉ mới thành công.");
        }

        public async Task<ServiceResponse> UpdateAddressAsync(Address addressToUpdate)
        {
            _context.Addresses.Update(addressToUpdate);
            await _context.SaveChangesAsync();
            return new ServiceResponse(true, "Cập nhật địa chỉ thành công.");
        }

        public async Task<ServiceResponse> SetDefaultAddressAsync(int addressId, int userId)
        {
            var currentDefault = await _context.Addresses
                .FirstOrDefaultAsync(a => a.UserID == userId && a.IsDefault);

            if (currentDefault != null)
            {
                currentDefault.IsDefault = false;
            }

            var newDefault = await _context.Addresses
                .FirstOrDefaultAsync(a => a.AddressID == addressId && a.UserID == userId);

            if (newDefault == null) return new ServiceResponse(false, "Không tìm thấy địa chỉ.");
            if (!newDefault.IsActive) return new ServiceResponse(false, "Không thể đặt địa chỉ đã xóa làm mặc định.");

            newDefault.IsDefault = true;
            await _context.SaveChangesAsync();
            return new ServiceResponse(true, "Đặt địa chỉ mặc định thành công.");
        }
        #endregion
    }
}