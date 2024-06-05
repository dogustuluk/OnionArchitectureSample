using ETicaretAPI.Application.Repositories;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ETicaretAPI.Application.Features.Commands.ProductImageFile.ChangeShowcaseProductImage
{
    public class ChangeShowcaseProductImageCommandHandler : IRequestHandler<ChangeShowcaseProductImageCommandRequest, ChangeShowcaseProductImageCommandResponse>
    {
        readonly IProductImageFileWriteRepository _productImageFileWriteRepository;

        public ChangeShowcaseProductImageCommandHandler(IProductImageFileWriteRepository productImageFileWriteRepository)
        {
            _productImageFileWriteRepository = productImageFileWriteRepository;
        }

        public async Task<ChangeShowcaseProductImageCommandResponse> Handle(ChangeShowcaseProductImageCommandRequest request, CancellationToken cancellationToken)
        {
            var query = _productImageFileWriteRepository.Table.Include(a => a.Products)
                .SelectMany(a => a.Products, (pif, p) => new
                {
                    pif,
                    p
                });
            var data = await query.FirstOrDefaultAsync(a => a.p.Id == Guid.Parse(request.ProductId) && a.pif.Showcase);

            if (data != null)
                data.pif.Showcase = false;

            var image = await query.FirstOrDefaultAsync(a => a.pif.Id == Guid.Parse(request.ImageId));
            if (image != null)
                image.pif.Showcase = true;

            await _productImageFileWriteRepository.SaveChanges();

            return new ChangeShowcaseProductImageCommandResponse();
        }
    }
}
