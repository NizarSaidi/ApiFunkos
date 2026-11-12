using System.Text.Json;
using FuncoApi.Cache;
using FuncoApi.Cache.objet;
using FuncoApi.Configure;
using FuncoApi.Context;
using FuncoApi.Context.Mongo;
using FuncoApi.Repositories;
using FuncoApi.Repositories.proveedoresRepository;
using FuncoApi.Services.CategoriaService;
using FuncoApi.Services.FunkosService;
using FuncoApi.Services.ProveedoresService;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using FunkoApi.Routes;

var builder = WebApplication.CreateBuilder(args);

// Mantiene los nombres JSON de la API en camelCase.
builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
});

// Selecciona la implementación de caché según el entorno.
if (ConfigurationApp.Mode == "Development")
{
    builder.Services.AddDistributedMemoryCache();
    builder.Services.AddScoped<
        ICacheService,
        RedisCacheService>();
}
else
{
    builder.Services.AddMemoryCache();
    builder.Services.AddScoped<
        ICacheService,
        MemoryCacheService>();
}

// Registra persistencia, servicios y repositorios que consumen las rutas.
builder.Services.AddSingleton<MongoDbContext>();
builder.Services.AddScoped<IProveedorDataSource, MongoProveedorDataSource>();
var sqlConnectionString = builder.Configuration.GetConnectionString("FuncoDb")
    ?? throw new InvalidOperationException("La cadena de conexión 'FuncoDb' no está configurada.");
builder.Services.AddDbContext<FuncoDbContext>(options =>
    options.UseSqlServer(sqlConnectionString));
builder.Services.AddScoped<IFunkoRepository, FunkoRepository>();
builder.Services.AddScoped<IFunkosService, FunkoService>();
builder.Services.AddScoped<IProveedorRepository, ProveedorRepository>();
builder.Services.AddScoped<ICategoriaRepository, CategoriaRepository>();
builder.Services.AddScoped<ICategoriaService, CategoriaService>();
builder.Services.AddScoped<IProveedorService, ProveedorService>();
builder.Services.AddControllers();

// autentificacion y autorizazion con JWT
var jwtBuilder = new JWTBuilder(builder);
jwtBuilder.ConfigureJWT();

var app = builder.Build();

// Crea el esquema SQL si todavía no existe.
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<FuncoDbContext>();
    await dbContext.Database.EnsureCreatedAsync();
}

// Habilita autenticación y autorización antes de procesar los controladores.
app.UseAuthentication();
app.UseAuthorization();

// Publica los controladores HTTP de la API.
app.MapControllers();

app.Run();