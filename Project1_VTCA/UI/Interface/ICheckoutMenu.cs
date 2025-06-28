using Project1_VTCA.Data;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Project1_VTCA.UI.Interface
{
    public interface ICheckoutMenu
    {
        Task StartCheckoutFlowAsync(List<CartItem> itemsToCheckout);
    }
}