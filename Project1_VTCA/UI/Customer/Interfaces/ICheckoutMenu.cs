using Project1_VTCA.Data;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Project1_VTCA.UI.Customer.Interfaces
{
    public interface ICheckoutMenu
    {
        
        Task<bool> StartCheckoutFlowAsync(List<CartItem> itemsToCheckout);
    }
}