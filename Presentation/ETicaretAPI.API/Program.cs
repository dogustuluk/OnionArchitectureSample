using ETicaretAPI.Application;
using ETicaretAPI.Application.Validators.Products;
using ETicaretAPI.Infrastructure;
using ETicaretAPI.Infrastructure.Enums;
using ETicaretAPI.Infrastructure.Filters;
using ETicaretAPI.Infrastructure.Services.Storage.Azure;
using ETicaretAPI.Infrastructure.Services.Storage.Local;
using ETicaretAPI.Persistence;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddPersistenceServices();
builder.Services.AddInfrastructureServices();
builder.Services.AddApplicationServices();

//dosya yönetimi storage mekanizmasý
builder.Services.AddStorage<AzureStorage>();
//builder.Services.AddStorage<LocalStorage>();
//builder.Services.AddStorage(StorageType.Local);

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

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer("Admin",options =>
    {
        options.TokenValidationParameters = new()
        {
            //doðrulama yapýlarý
            ValidateAudience = true, //oluþturulacak token'ýn hangi site tarafýndan kullanýlacaðýný belirtir. "www.dogustuluk.com" gibi
            ValidateIssuer = true, //oluþturulacak token'ýn kimin daðýttýðýný ifade eder. "www.olusturulanAPI.com" þeklinde
            ValidateLifetime = true, //oluþturulan token'ýn süresini kontrol edecek doðrulama
            ValidateIssuerSigningKey = true, // oluþturulan token'ýn security key'inin doðrulanmasý

            //hangi deðerlerle doðrulanacaðý
            ValidAudience = builder.Configuration["Token:Audience"],
            ValidIssuer = builder.Configuration["Token:Issuer"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Token:SecurityKey"]))

        };
    });

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseStaticFiles();
app.UseCors();//cors için middleware ekle.

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
