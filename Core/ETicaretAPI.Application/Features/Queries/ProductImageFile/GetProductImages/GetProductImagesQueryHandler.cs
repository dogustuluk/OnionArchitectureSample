using ETicaretAPI.Application.Repositories;
using A = ETicaretAPI.Domain.Entities;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;

namespace ETicaretAPI.Application.Features.Queries.ProductImageFile.GetProductImages
{
    public class GetProductImagesQueryHandler : IRequestHandler<GetProductImagesQueryRequest, List<GetProductImagesQueryResponse>>
    {
        readonly IProductReadRepository _productReadRepository;
        readonly IConfiguration _configuration;

        public GetProductImagesQueryHandler(IProductReadRepository productReadRepository, IConfiguration configuration)
        {
            _productReadRepository = productReadRepository;
            _configuration = configuration;
        }

        public async Task<List<GetProductImagesQueryResponse>> Handle(GetProductImagesQueryRequest request, CancellationToken cancellationToken)
        {

            Domain.Entities.Product? product = await _productReadRepository.Table.Include(a => a.ProductImageFiles).FirstOrDefaultAsync(x => x.Id == Guid.Parse(request.Id));

            return product?.ProductImageFiles.Select(a => new GetProductImagesQueryResponse
            {
                Path = $"{_configuration["BaseStorageUrl"]}/{a.Path}",
                FileName = a.FileName,
                Id = a.Id
            }).ToList();
        }
    }
}
