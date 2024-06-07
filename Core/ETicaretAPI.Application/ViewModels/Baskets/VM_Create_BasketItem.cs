using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ETicaretAPI.Application.ViewModels.Baskets
{
    public class VM_Create_BasketItem
    {
        //burada string BasketId alabilirdik ama doğru olan yaklaşım o anda oturumu açık olan kullanıcının aktif olan basket'i hangisiyse ona eklememiz gerekir.
        public string ProductId { get; set; }
        public int Quantity { get; set; }
    }
}
