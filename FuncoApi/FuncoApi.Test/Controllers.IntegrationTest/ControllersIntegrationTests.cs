using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FuncoApi.Cache;
using FuncoApi.Cache.objet;
using FuncoApi.Context;
using FuncoApi.Context.Mongo;
using FuncoApi.Controlers;
using FuncoApi.Email.service;
using FuncoApi.Repositories;
using FuncoApi.Repositories.proveedoresRepository;
using FuncoApi.Repositories.userRepository;
using FuncoApi.Services.CategoriaService;
using FuncoApi.Services.FunkosService;
using FuncoApi.Services.ProveedoresService;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Mongo2Go;

namespace FuncoApi.Test.Controllers.IntegrationTest;

/// <summary>Prueba los endpoints HTTP usando MVC, servicios y persistencia reales.</summary>
public class ControllersIntegrationTests : IAsyncLifetime
{
    private MongoDbRunner _mongoRunner = null!;

    public Task InitializeAsync()
    {
        _mongoRunner = MongoDbRunner.Start(singleNodeReplSet: false);
        return Task.CompletedTask;
    }

    public Task DisposeAsync()
    {
        _mongoRunner.Dispose();
        return Task.CompletedTask;
    }

    [Fact]
    public async Task Controllers_ExposeCrudAndDomainErrorsOverHttp()
    {
        await using var host = await StartApiAsync();
        using var client = host.Client;

        var categoryResponse = await client.PostAsJsonAsync("/api/categorias",
            new { nombre = "Anime" });
        Assert.True(categoryResponse.StatusCode == HttpStatusCode.Created,
            $"Expected Created but received {(int)categoryResponse.StatusCode}: " +
            await categoryResponse.Content.ReadAsStringAsync());
        var category = await ReadJsonAsync(categoryResponse);
        var categoryId = category.GetProperty("id").GetString();
        Assert.False(string.IsNullOrWhiteSpace(categoryId));
        Assert.EndsWith($"/api/categorias/{categoryId}", categoryResponse.Headers.Location!.ToString());

        var duplicateCategory = await client.PostAsJsonAsync("/api/categorias",
            new { nombre = "Anime" });
        Assert.Equal(HttpStatusCode.Conflict, duplicateCategory.StatusCode);

        var categoryListResponse = await client.GetAsync("/api/categorias?page=1");
        Assert.Equal(HttpStatusCode.OK, categoryListResponse.StatusCode);
        var categoryList = await ReadJsonAsync(categoryListResponse);
        Assert.Equal(1, categoryList.GetProperty("totalCount").GetInt32());
        Assert.Single(categoryList.GetProperty("data").EnumerateArray());

        var providerResponse = await client.PostAsJsonAsync("/api/proveedores",
            new { nombre = "Distribuidor" });
        Assert.Equal(HttpStatusCode.Created, providerResponse.StatusCode);
        var provider = await ReadJsonAsync(providerResponse);
        var providerId = provider.GetProperty("id").GetInt32();
        Assert.True(providerId > 0);
        Assert.EndsWith($"/api/proveedores/{providerId}", providerResponse.Headers.Location!.ToString());

        var duplicateProvider = await client.PostAsJsonAsync("/api/proveedores",
            new { nombre = "Distribuidor" });
        Assert.Equal(HttpStatusCode.Conflict, duplicateProvider.StatusCode);

        var providerListResponse = await client.GetAsync("/api/proveedores?page=1");
        Assert.Equal(HttpStatusCode.OK, providerListResponse.StatusCode);
        var providerList = await ReadJsonAsync(providerListResponse);
        Assert.Equal(1, providerList.GetProperty("totalCount").GetInt32());
        Assert.Single(providerList.GetProperty("data").EnumerateArray());

        var funkoPayload = new
        {
            nombre = "Spider-Man",
            imagen = "spiderman.png",
            precio = 15.5m,
            stock = 4,
            categoria = categoryId,
            proveedor = providerId
        };
        var funkoResponse = await client.PostAsJsonAsync("/api/funcopops", funkoPayload);
        Assert.True(funkoResponse.StatusCode == HttpStatusCode.Created,
            $"Expected Created but received {(int)funkoResponse.StatusCode}: " +
            await funkoResponse.Content.ReadAsStringAsync());
        var funko = await ReadJsonAsync(funkoResponse);
        var funkoId = funko.GetProperty("id").GetInt32();
        Assert.True(funkoId > 0);
        Assert.EndsWith($"/api/funcopops/{funkoId}", funkoResponse.Headers.Location!.ToString());
        Assert.Equal(categoryId, funko.GetProperty("categoria").GetString());

        var duplicateFunko = await client.PostAsJsonAsync("/api/funcopops", funkoPayload);
        Assert.Equal(HttpStatusCode.Conflict, duplicateFunko.StatusCode);

        var funkoListResponse = await client.GetAsync("/api/funcopops?page=1");
        Assert.Equal(HttpStatusCode.OK, funkoListResponse.StatusCode);
        var funkoList = await ReadJsonAsync(funkoListResponse);
        Assert.Equal(1, funkoList.GetProperty("totalCount").GetInt32());
        Assert.Single(funkoList.GetProperty("data").EnumerateArray());

        var updatedFunkoResponse = await client.PutAsJsonAsync($"/api/funcopops/{funkoId}",
            new
            {
                nombre = "Spider-Man actualizado",
                imagen = "spiderman-v2.png",
                precio = 16m,
                stock = 5,
                categoria = categoryId,
                proveedor = providerId
            });
        Assert.Equal(HttpStatusCode.OK, updatedFunkoResponse.StatusCode);
        var updatedFunko = await ReadJsonAsync(updatedFunkoResponse);
        Assert.Equal("Spider-Man actualizado", updatedFunko.GetProperty("nombre").GetString());

        Assert.Equal(HttpStatusCode.OK,
            (await client.GetAsync($"/api/funcopops/{funkoId}")).StatusCode);
        Assert.Equal(HttpStatusCode.OK,
            (await client.DeleteAsync($"/api/funcopops/{funkoId}")).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound,
            (await client.GetAsync($"/api/funcopops/{funkoId}")).StatusCode);

        Assert.Equal(HttpStatusCode.OK,
            (await client.PutAsJsonAsync($"/api/categorias/{categoryId}",
                new { nombre = "Animación" })).StatusCode);
        Assert.Equal(HttpStatusCode.OK,
            (await client.GetAsync($"/api/categorias/{categoryId}")).StatusCode);
        Assert.Equal(HttpStatusCode.OK,
            (await client.DeleteAsync($"/api/categorias/{categoryId}")).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound,
            (await client.GetAsync($"/api/categorias/{categoryId}")).StatusCode);

        Assert.Equal(HttpStatusCode.OK,
            (await client.PutAsJsonAsync($"/api/proveedores/{providerId}",
                new { nombre = "Distribuidor actualizado" })).StatusCode);
        Assert.Equal(HttpStatusCode.OK,
            (await client.GetAsync($"/api/proveedores/{providerId}")).StatusCode);
        Assert.Equal(HttpStatusCode.OK,
            (await client.DeleteAsync($"/api/proveedores/{providerId}")).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound,
            (await client.GetAsync($"/api/proveedores/{providerId}")).StatusCode);

        var invalidFunko = await client.PostAsJsonAsync("/api/funcopops", new
        {
            nombre = "",
            imagen = "",
            precio = 0,
            stock = -1,
            categoria = "",
            proveedor = 0
        });
        Assert.Equal(HttpStatusCode.BadRequest, invalidFunko.StatusCode);
    }

    private async Task<TestApiHost> StartApiAsync()
    {
        var databaseName = $"controllers_{Guid.NewGuid():N}";
        var builder = WebApplication.CreateBuilder(new WebApplicationOptions
        {
            EnvironmentName = "Testing"
        });
        builder.WebHost.ConfigureKestrel(options =>
            options.Listen(System.Net.IPAddress.Loopback, 0));
        builder.Configuration.AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["MongoSettings:ConnectionString"] = _mongoRunner.ConnectionString,
            ["MongoSettings:DatabaseName"] = $"controllers_{Guid.NewGuid():N}"
        });

        builder.Services.AddDbContext<FuncoDbContext>(options =>
            options.UseInMemoryDatabase(databaseName));
        builder.Services.AddMemoryCache();
        builder.Services.AddScoped<ICacheService, MemoryCacheService>();
        builder.Services.AddSingleton<MongoDbContext>();
        builder.Services.AddScoped<IProveedorDataSource, MongoProveedorDataSource>();
        builder.Services.AddScoped<IFunkoRepository, FunkoRepository>();
        builder.Services.AddScoped<ICategoriaRepository, CategoriaRepository>();
        builder.Services.AddScoped<IProveedorRepository, ProveedorRepository>();
        builder.Services.AddScoped<IUserRepository, UsuarioRepository>();
        builder.Services.AddSingleton<IEmailService>(_ => Moq.Mock.Of<IEmailService>());
        builder.Services.AddScoped<IFunkosService, FunkoService>();
        builder.Services.AddScoped<ICategoriaService, CategoriaService>();
        builder.Services.AddScoped<IProveedorService, ProveedorService>();
        builder.Services
            .AddAuthentication("IntegrationTest")
            .AddScheme<AuthenticationSchemeOptions, IntegrationTestAuthHandler>(
                "IntegrationTest", _ => { });
        builder.Services.AddAuthorization();
        builder.Services.AddControllers()
            .AddApplicationPart(typeof(FunkosControler).Assembly);

        var app = builder.Build();
        app.UseAuthentication();
        app.UseAuthorization();
        app.MapControllers();
        await app.StartAsync();

        var server = app.Services.GetRequiredService<IServer>();
        var address = server.Features.Get<IServerAddressesFeature>()!.Addresses.Single();
        return new TestApiHost(app, new HttpClient { BaseAddress = new Uri(address) });
    }

    private static async Task<JsonElement> ReadJsonAsync(HttpResponseMessage response)
    {
        await using var content = await response.Content.ReadAsStreamAsync();
        using var document = await JsonDocument.ParseAsync(content);
        return document.RootElement.Clone();
    }

    private sealed class TestApiHost(WebApplication application, HttpClient client) : IAsyncDisposable
    {
        public HttpClient Client { get; } = client;

        public async ValueTask DisposeAsync()
        {
            Client.Dispose();
            await application.DisposeAsync();
        }
    }

    private sealed class IntegrationTestAuthHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder)
        : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
    {
        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            var identity = new ClaimsIdentity(
                [new Claim(ClaimTypes.Name, "integration-admin"),
                 new Claim(ClaimTypes.Role, "Admin")],
                Scheme.Name);
            var principal = new ClaimsPrincipal(identity);
            var ticket = new AuthenticationTicket(principal, Scheme.Name);
            return Task.FromResult(AuthenticateResult.Success(ticket));
        }
    }
}
