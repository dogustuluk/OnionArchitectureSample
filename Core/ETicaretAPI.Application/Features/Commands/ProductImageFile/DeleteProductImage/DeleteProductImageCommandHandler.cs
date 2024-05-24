using ETicaretAPI.Application.Repositories;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace ETicaretAPI.Application.Features.Commands.ProductImageFile.DeleteProductImage
{
    public class DeleteProductImageCommandHandler : IRequestHandler<DeleteProductImageCommandRequest, DeleteProductImageCommandResponse>
    {
        readonly IProductReadRepository _productReadRepository;
        readonly IProductWriteRepository _productWriteRepository;

        public DeleteProductImageCommandHandler(IProductReadRepository productReadRepository, IProductWriteRepository productWriteRepository)
        {
            _productReadRepository = productReadRepository;
            _productWriteRepository = productWriteRepository;
        }

        public async Task<DeleteProductImageCommandResponse> Handle(DeleteProductImageCommandRequest request, CancellationToken cancellationToken)
        {
            Domain.Entities.Product? product = await _productReadRepository.Table.Include(a => a.ProductImageFiles).FirstOrDefaultAsync(x => x.Id == Guid.Parse(request.Id));

            Domain.Entities.ProductImageFile? productImageFile = product?.ProductImageFiles.FirstOrDefault(a => a.Id == Guid.Parse(request.ImageId));

            if (productImageFile != null)
                product?.ProductImageFiles.Remove(productImageFile);
            await _productWriteRepository.SaveChanges();

            return new();
        }
    }
}
