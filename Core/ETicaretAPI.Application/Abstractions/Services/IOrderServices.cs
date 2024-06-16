using ETicaretAPI.Application.DTOs.Order;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ETicaretAPI.Application.Abstractions.Services
{
    public interface IOrderServices
    {
        Task CreateOrder(CreateOrder createOrder);
        Task<ListOrder> GetAllOrdersAsync(int page, int size);
    }
}
