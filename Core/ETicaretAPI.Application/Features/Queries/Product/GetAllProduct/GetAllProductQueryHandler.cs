using ETicaretAPI.Application.Repositories;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

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
            //throw new Exception("test!! global exception handler");
            var totalProductCount = _productReadRepository.GetAll(false).Count();
            var products = _productReadRepository.GetAll(false).Skip(request.Page * request.Size).Take(request.Size)
                .Include(a => a.ProductImageFiles)
                .Select(a => new
            {
                a.Id,
                a.Name,
                a.Price,
                a.Stock,
                a.CreatedDate,
                a.UpdatedDate,
                a.ProductImageFiles
            }).ToList(); //track etmeye gerek yok çünkü üzerinde işlem yapılmıyor sadece kullanıcıya sunuluyor
            _logger.LogInformation("Tüm product'lar listelendi");

            return new()
            {
                Products = products,
                TotalProductCount = totalProductCount
            };


        }
    }
}
