using ETicaretAPI.Application.Abstractions;
using ETicaretAPI.Application.Repositories;
using MediatR;

namespace ETicaretAPI.Application.Features.Commands.ProductImageFile.UploadProductImage
{
    public class UploadProductImageCommandHandler : IRequestHandler<UploadProductImageCommandRequest, UploadProductImageCommandResponse>
    {
        readonly IStorageService _storageService;
        readonly IProductReadRepository _productReadRepository;
        readonly IProductImageFileWriteRepository _productImageFileWriteRepository;

        public UploadProductImageCommandHandler(IStorageService storageService, IProductReadRepository productReadRepository, IProductImageFileWriteRepository productImageFileWriteRepository)
        {
            _storageService = storageService;
            _productReadRepository = productReadRepository;
            _productImageFileWriteRepository = productImageFileWriteRepository;
        }

        public async Task<UploadProductImageCommandResponse> Handle(UploadProductImageCommandRequest request, CancellationToken cancellationToken)
        {
            List<(string fileName, string pathOrContainerName)> result = await _storageService.UploadAsync("photo-image",request.Files); //upload async metodunu buradaki gibi tupple olarak göndermek doğru olmayabilir bunun yerine tip güvenli olarak view model şeklinde dön.

            Domain.Entities.Product product = await _productReadRepository.GetByIdAsync(request.Id);

            await _productImageFileWriteRepository.AddRangeAsync(result.Select(a => new Domain.Entities.ProductImageFile
            {
                FileName = a.fileName,
                Path = a.pathOrContainerName,
                Storage = _storageService.StorageName,
                Products = new List<Domain.Entities.Product>() { product }
            }).ToList());

            await _productImageFileWriteRepository.SaveChanges();

            return new();
        }
    }
}
