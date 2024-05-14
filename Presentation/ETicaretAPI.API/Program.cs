using ETicaretAPI.Application.Validators.Products;
using ETicaretAPI.Infrastructure.Filters;
using ETicaretAPI.Persistence;
using FluentValidation.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddPersistenceServices();

//cors politikasý belirleme
//builder.Services.AddCors(options => options.AddDefaultPolicy(policy => policy.AllowAnyHeader().AllowAnyMethod().AllowAnyOrigin())); //bu þekilde her istek gelecektir. istenilen durum deðil.
builder.Services.AddCors(options => options.AddDefaultPolicy(policy => policy.WithOrigins("http://localhost:4200", "https://localhost:4200").AllowAnyHeader().AllowAnyMethod()));

//RegisterValidatorsFromAssemblyContaining ile her bir validator sýnýfýný tek tek yazmayýz.
builder.Services.AddControllers(options => options.Filters.Add<ValidationFilter>())
	.AddFluentValidation(configuration => configuration.RegisterValidatorsFromAssemblyContaining<CreateProductValidator>())
	
	//mevcut olanýn dýþýnda benim kendi yazacaðým filter'larý devreye sok özelliði için true yapýlýr. custom validation filter istersek.
	.ConfigureApiBehaviorOptions(options => options.SuppressModelStateInvalidFilter = true);
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
	app.UseSwagger();
	app.UseSwaggerUI();
}

app.UseCors();//cors için middleware ekle.

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
