using System.Threading.Tasks;
using Project1_VTCA.Data;

namespace Project1_VTCA.UI.Customer.Interfaces
{
    public interface IAddressMenu
    {
        Task ShowAddressManagementAsync();
        Task<Address?> HandleAddAddressFlowAsync(bool setDefault = false);
    }
}