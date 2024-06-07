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

builder.Services.AddHttpContextAccessor(); //bu servis client'tan gelen requestler sonucu olu�an httpcontext nesnesine katmanlardaki class'lar �zerinden(bussiness logic'ler) eri�memizi sa�lamaktad�r.
builder.Services.AddPersistenceServices();
builder.Services.AddInfrastructureServices();
builder.Services.AddApplicationServices();
builder.Services.AddSignalRServices();

//dosya y�netimi storage mekanizmas�
builder.Services.AddStorage<AzureStorage>();
//builder.Services.AddStorage<LocalStorage>();
//builder.Services.AddStorage(StorageType.Local);

//cors politikas� belirleme
//builder.Services.AddCors(options => options.AddDefaultPolicy(policy => policy.AllowAnyHeader().AllowAnyMethod().AllowAnyOrigin())); //bu �ekilde her istek gelecektir. istenilen durum de�il.
builder.Services.AddCors(options => options.AddDefaultPolicy(policy => policy.WithOrigins("http://localhost:4200", "https://localhost:4200").AllowAnyHeader().AllowAnyMethod().AllowCredentials()));

//log
Logger log = new LoggerConfiguration()
    //araya konfig�rasyonlar eklenir.
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
    .Enrich.FromLogContext() //�zel property'ler varsa yazmam�z gerekir (bknz. user_name)
    .MinimumLevel.Information()
    .CreateLogger();
builder.Host.UseSerilog(log);

//http istekleri i�in httplogging
builder.Services.AddHttpLogging(logging =>
{
    logging.LoggingFields = HttpLoggingFields.All;
    logging.RequestHeaders.Add("sec-ch-ua"); //kullan�c�ya dair t�m teferr�atl� bilgileri getirir.
    logging.MediaTypeOptions.AddText("application/javascript");
    logging.RequestBodyLogLimit = 4096;
    logging.ResponseBodyLogLimit = 4096;
});


//RegisterValidatorsFromAssemblyContaining ile her bir validator s�n�f�n� tek tek yazmay�z.
builder.Services.AddControllers(options => options.Filters.Add<ValidationFilter>())
    .AddFluentValidation(configuration => configuration.RegisterValidatorsFromAssemblyContaining<CreateProductValidator>())

    //mevcut olan�n d���nda benim kendi yazaca��m filter'lar� devreye sok �zelli�i i�in true yap�l�r. custom validation filter istersek.
    .ConfigureApiBehaviorOptions(options => options.SuppressModelStateInvalidFilter = true);
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer("Admin", options =>
    {
        options.TokenValidationParameters = new()
        {
            //do�rulama yap�lar�
            ValidateAudience = true, //olu�turulacak token'�n hangi site taraf�ndan kullan�laca��n� belirtir. "www.dogustuluk.com" gibi
            ValidateIssuer = true, //olu�turulacak token'�n kimin da��tt���n� ifade eder. "www.olusturulanAPI.com" �eklinde
            ValidateLifetime = true, //olu�turulan token'�n s�resini kontrol edecek do�rulama
            ValidateIssuerSigningKey = true, // olu�turulan token'�n security key'inin do�rulanmas�

            //hangi de�erlerle do�rulanaca��
            ValidAudience = builder.Configuration["Token:Audience"],
            ValidIssuer = builder.Configuration["Token:Issuer"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Token:SecurityKey"])),
            LifetimeValidator = (notBefore, expires, securityToken, validationParameters) => expires != null ? expires > DateTime.UtcNow : false, //expiration time i�in.

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

app.UseSerilogRequestLogging();//bu middleware kendisinden �nceki t�m middleware'leri loglamaz.
app.UseHttpLogging();//yap�lan request'leri de yakalar�z.
app.UseCors();//cors i�in middleware ekle.

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

//midleware
app.Use(async (context, next) =>
{
    var username = context.User?.Identity?.IsAuthenticated != null || true ? context.User.Identity.Name : null;

    //property olu�tur ve log mekanizmas�n�n context'ine atmak i�in
    LogContext.PushProperty("user_name", username);

    await next(); //bir di�er middleware'e ge�mesi, ak���n devam� i�in gerekli
});

app.MapControllers();

//signalR hub'lar� tan�t�lmal�
app.MapHubs();

app.Run();
