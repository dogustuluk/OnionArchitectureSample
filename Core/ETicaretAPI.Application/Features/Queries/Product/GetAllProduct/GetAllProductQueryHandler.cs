using ETicaretAPI.Application.Repositories;
using ETicaretAPI.Application.RequestParameters;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ETicaretAPI.Application.Features.Queries.Product.GetAllProduct
{
    public class GetAllProductQueryHandler : IRequestHandler<GetAllProductQueryRequest, GetAllProductQueryResponse>
    {
        readonly IProductReadRepository _productReadRepository;
        readonly ILogger<GetAllProductQueryHandler> _logger;
        public GetAllProductQueryHandler(IProductReadRepository productReadRepository, ILogger<GetAllProductQueryHandler> logger)
        {
            _productReadRepository = productReadRepository;
            _logger = logger;
        }

        public async Task<GetAllProductQueryResponse> Handle(GetAllProductQueryRequest request, CancellationToken cancellationToken)
        {
            var totalCount = _productReadRepository.GetAll(false).Count();
            var products = _productReadRepository.GetAll(false).Skip(request.Page * request.Size).Take(request.Size).Select(a => new
            {
                a.Id,
                a.Name,
                a.Price,
                a.Stock,
                a.CreatedDate,
                a.UpdatedDate
            }).ToList(); //track etmeye gerek yok çünkü üzerinde işlem yapılmıyor sadece kullanıcıya sunuluyor
            _logger.LogInformation("Tüm product'lar listelendi");

            return new()
            {
                Products = products,
                TotalCount = totalCount
            };


        }
    }
}
