using ETicaretAPI.Application.ViewModels.Products;
using FluentValidation;

namespace ETicaretAPI.Application.Validators.Products
{
    public class CreateProductValidator: AbstractValidator<VM_Create_Product>
    {
        public CreateProductValidator()
        {
            RuleFor(a => a.Name)
                .NotEmpty()
                .NotNull()
                    .WithMessage("Lütfen ürün adını boş geçmeyiniz")
                .MaximumLength(150)
                .MinimumLength(5)
                    .WithMessage("Lütfen ürün adını 5 ile 150 karakter arasında giriniz");

            RuleFor(a => a.Stock)
                .NotEmpty()
                .NotNull()
                    .WithMessage("Lütfen stok bilgisini boş geçmeyiniz")
                .Must(x => x >= 0)
                .WithMessage("Stok bilgisi negatif bir değer olamaz");
          
            RuleFor(a => a.Price)
                .NotEmpty()
                .NotNull()
                    .WithMessage("Lütfen fiyat bilgisini boş geçmeyiniz")
                .Must(x => x >= 0)
                .WithMessage("fiyat bilgisi negatif bir değer olamaz");
        }
    }
}
