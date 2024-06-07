using ETicaretAPI.API.Configurations.ColumnWriters;
using ETicaretAPI.API.Extensions;
using ETicaretAPI.Application;
using ETicaretAPI.Application.Validators.Products;
using ETicaretAPI.Infrastructure;
using ETicaretAPI.Infrastructure.Filters;
using ETicaretAPI.Infrastructure.Services.Storage.Azure;
using ETicaretAPI.Persistence;
using ETicaretAPI.SignalR;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.HttpLogging;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using Serilog.Context;
using Serilog.Core;
using Serilog.Sinks.PostgreSQL;
using System.Security.Claims;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHttpContextAccessor(); //bu servis client'tan gelen requestler sonucu oluþan httpcontext nesnesine katmanlardaki class'lar üzerinden(bussiness logic'ler) eriþmemizi saðlamaktadýr.
builder.Services.AddPersistenceServices();
builder.Services.AddInfrastructureServices();
builder.Services.AddApplicationServices();
builder.Services.AddSignalRServices();

//dosya yönetimi storage mekanizmasý
builder.Services.AddStorage<AzureStorage>();
//builder.Services.AddStorage<LocalStorage>();
//builder.Services.AddStorage(StorageType.Local);

//cors politikasý belirleme
//builder.Services.AddCors(options => options.AddDefaultPolicy(policy => policy.AllowAnyHeader().AllowAnyMethod().AllowAnyOrigin())); //bu þekilde her istek gelecektir. istenilen durum deðil.
builder.Services.AddCors(options => options.AddDefaultPolicy(policy => policy.WithOrigins("http://localhost:4200", "https://localhost:4200").AllowAnyHeader().AllowAnyMethod().AllowCredentials()));

//log
Logger log = new LoggerConfiguration()
    //araya konfigürasyonlar eklenir.
    .WriteTo.Console()
    .WriteTo.File("logs/log.txt")
    .WriteTo.PostgreSQL(builder.Configuration.GetConnectionString("PostgreSQL"), "logs",
        needAutoCreateTable: true,
        columnOptions: new Dictionary<string, ColumnWriterBase>
        {
            {"message", new RenderedMessageColumnWriter() },
            {"message_template", new MessageTemplateColumnWriter() },
            {"level", new LevelColumnWriter() },
            {"timestamp", new TimestampColumnWriter() },
            {"exception", new ExceptionColumnWriter() },
            {"log_event", new LogEventSerializedColumnWriter() },
            {"properties", new PropertiesColumnWriter() },
            {"user_name", new UsernameColumnWriter() } //custom
        })
    .WriteTo.Seq(builder.Configuration["Seq:ServerURL"])
    .Enrich.FromLogContext() //özel property'ler varsa yazmamýz gerekir (bknz. user_name)
    .MinimumLevel.Information()
    .CreateLogger();
builder.Host.UseSerilog(log);

//http istekleri için httplogging
builder.Services.AddHttpLogging(logging =>
{
    logging.LoggingFields = HttpLoggingFields.All;
    logging.RequestHeaders.Add("sec-ch-ua"); //kullanýcýya dair tüm teferrüatlý bilgileri getirir.
    logging.MediaTypeOptions.AddText("application/javascript");
    logging.RequestBodyLogLimit = 4096;
    logging.ResponseBodyLogLimit = 4096;
});


//RegisterValidatorsFromAssemblyContaining ile her bir validator sýnýfýný tek tek yazmayýz.
builder.Services.AddControllers(options => options.Filters.Add<ValidationFilter>())
    .AddFluentValidation(configuration => configuration.RegisterValidatorsFromAssemblyContaining<CreateProductValidator>())

    //mevcut olanýn dýþýnda benim kendi yazacaðým filter'larý devreye sok özelliði için true yapýlýr. custom validation filter istersek.
    .ConfigureApiBehaviorOptions(options => options.SuppressModelStateInvalidFilter = true);
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer("Admin", options =>
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
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Token:SecurityKey"])),
            LifetimeValidator = (notBefore, expires, securityToken, validationParameters) => expires != null ? expires > DateTime.UtcNow : false, //expiration time için.

            //claims
            NameClaimType = ClaimTypes.Name
        };
    });

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.ConfigureExceptionHandler<Program>(app.Services.GetRequiredService<ILogger<Program>>());//global exception handler
app.UseStaticFiles();

app.UseSerilogRequestLogging();//bu middleware kendisinden önceki tüm middleware'leri loglamaz.
app.UseHttpLogging();//yapýlan request'leri de yakalarýz.
app.UseCors();//cors için middleware ekle.

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

//midleware
app.Use(async (context, next) =>
{
    var username = context.User?.Identity?.IsAuthenticated != null || true ? context.User.Identity.Name : null;

    //property oluþtur ve log mekanizmasýnýn context'ine atmak için
    LogContext.PushProperty("user_name", username);

    await next(); //bir diðer middleware'e geçmesi, akýþýn devamý için gerekli
});

app.MapControllers();

//signalR hub'larý tanýtýlmalý
app.MapHubs();

app.Run();
