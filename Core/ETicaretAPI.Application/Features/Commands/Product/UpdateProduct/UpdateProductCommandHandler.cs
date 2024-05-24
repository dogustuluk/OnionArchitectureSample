using ETicaretAPI.Application.Repositories;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using A = ETicaretAPI.Domain.Entities;
namespace ETicaretAPI.Application.Features.Commands.Product.UpdateProduct
{
    public class UpdateProductCommandHandler : IRequestHandler<UpdateProductCommandRequest, UpdateProductCommandResponse>
    {
        readonly IProductReadRepository _productReadRepository;
        readonly IProductWriteRepository _productWriteRepository;

        public UpdateProductCommandHandler(IProductReadRepository productReadRepository, IProductWriteRepository productWriteRepository)
        {
            _productReadRepository = productReadRepository;
            _productWriteRepository = productWriteRepository;
        }

        public async Task<UpdateProductCommandResponse> Handle(UpdateProductCommandRequest request, CancellationToken cancellationToken)
        {
            A.Product product = await _productReadRepository.GetByIdAsync(request.Id);
            product.Stock = request.Stock;
            product.Price = request.Price;
            product.Name = request.Name;
            await _productWriteRepository.SaveChanges();

            return new()
            {
                Id = request.Id,
                Name = product.Name,
                Stock = product.Stock,
                Price = product.Price,
            };
        }
    }
}
