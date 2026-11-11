using System.Text.Json;
using FuncoApi.Cache;
using FuncoApi.Cache.objet;
using FuncoApi.Configure;
using FuncoApi.Context;
using FuncoApi.Context.Mongo;
using FuncoApi.Repositories;
using FuncoApi.Repositories.proveedoresRepository;
using FunkoApi.Routes;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;

var builder = WebApplication.CreateBuilder(args);

builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
});

if (ConfigurationApp.Mode == "Development")
{
    builder.Services.AddScoped<
        ICacheService,
        RedisCacheService>();
}
else
{
    builder.Services.AddScoped<
        ICacheService,
        MemoryCacheService>();
}

builder.Services.AddSingleton<MongoDbContext>();
builder.Services.AddScoped<IProveedorDataSource, MongoProveedorDataSource>();

// Añadir scoed y cofihguracion    
builder.Services.AddDbContext<FuncoDbContext>(options => 
    builder.Configuration.GetConnectionString(ConfigurationApp.Mode));
builder.Services.AddScoped<IFunkoRepository, FunkoRepository>();
builder.Services.AddScoped<IProveedorRepository, ProveedorRepository>();
builder.Services.AddScoped<ICategoriaRepository, CategoriaRepository>();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<FuncoDbContext>();
    await dbContext.Database.EnsureCreatedAsync();
}

app.MapFunkosRoutes();

app.Run();