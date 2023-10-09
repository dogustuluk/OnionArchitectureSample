using ETicaretAPI.Application.Repositories;
using ETicaretAPI.Domain.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ETicaretAPI.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductsController : ControllerBase
    {
        readonly private IProductWriteRepository _productWriteRepository;
        readonly private IProductReadRepository _productReadRepository;

		readonly private IOrderWriteRepository _orderWriteRepository;
		readonly private ICustomerWriteRepository _customerWriteRepository;

		public ProductsController(IProductWriteRepository productWriteRepository, IProductReadRepository productReadRepository, IOrderWriteRepository orderWriteRepository, ICustomerWriteRepository customerWriteRepository)
		{
			_productWriteRepository = productWriteRepository;
			_productReadRepository = productReadRepository;
			_orderWriteRepository = orderWriteRepository;
			_customerWriteRepository = customerWriteRepository;
		}

		[HttpGet("Get")]
        public async Task Get()
        {
            var customerId = Guid.NewGuid();
            await _customerWriteRepository.AddAsync(new() { Id=customerId, Name="Doğuş"  });
            await _orderWriteRepository.AddAsync(new() {  Description="test test test", Address="İzmir,Konak", CustomerId= customerId });
            await _orderWriteRepository.AddAsync(new() {  Description="test2 test2 test2", Address="İzmir,Konak", CustomerId= customerId });
            await _orderWriteRepository.SaveChanges();
        }
    }
}
