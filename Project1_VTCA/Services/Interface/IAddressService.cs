using Project1_VTCA.Data;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Project1_VTCA.Services.Interface
{
    public interface IAddressService
    {
        Task<List<Address>> GetActiveAddressesAsync(int userId);
        Task<(ServiceResponse Response, Address? NewAddress)> AddAddressAsync(Address newAddress);
        Task<ServiceResponse> UpdateAddressAsync(Address addressToUpdate);
        Task<ServiceResponse> SoftDeleteAddressAsync(int addressId, int userId);
        Task<ServiceResponse> SetDefaultAddressAsync(int addressId, int userId);
    }
}