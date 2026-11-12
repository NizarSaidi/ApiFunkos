using FuncoApi.Domain;
using FuncoApi.Entity;
using FuncoApi.Models;
using FuncoApi.Repositories;
using FuncoApi.Repositories.proveedoresRepository;
using FuncoApi.Repositories.userRepository;

namespace FuncoApi.Data;

/// <summary>Inserta datos de ejemplo mediante los repositorios en entorno de desarrollo.</summary>
public static class DevelopmentDataSeeder
{
    private const int LookupPageSize = 500;
    private const string AdminEmail = "admin@funco.local";
    private const string AdminName = "Admin local";

    /// <summary>Inicializa datos de prueba sin duplicarlos en ejecuciones posteriores.</summary>
    /// <param name="services">Proveedor de servicios de la aplicación.</param>
    /// <param name="configuration">Configuración que puede incluir la contraseña del Admin local.</param>
    /// <param name="logger">Registrador para informar los datos inicializados.</param>
    public static async Task SeedAsync(
        IServiceProvider services,
        IConfiguration configuration,
        ILogger logger)
    {
        using var scope = services.CreateScope();
        var users = scope.ServiceProvider.GetRequiredService<IUserRepository>();
        var categories = scope.ServiceProvider.GetRequiredService<ICategoriaRepository>();
        var providers = scope.ServiceProvider.GetRequiredService<IProveedorRepository>();
        var funkos = scope.ServiceProvider.GetRequiredService<IFunkoRepository>();

        await SeedAdminAsync(users, configuration, logger);

        var category = await EnsureCategoryAsync(categories, "Figuras clásicas");
        var animeCategory = await EnsureCategoryAsync(categories, "Anime");
        var provider = await EnsureProviderAsync(providers, "Proveedor de prueba");
        var animeProvider = await EnsureProviderAsync(providers, "Distribuidor de prueba");

        await EnsureFunkoAsync(funkos, "Funko de prueba", category.Id, provider.Id, 12.50m, 10);
        await EnsureFunkoAsync(funkos, "Funko anime de prueba", animeCategory.Id, animeProvider.Id, 15.99m, 5);

        logger.LogInformation("Datos de desarrollo inicializados mediante los repositorios.");
    }

    private static async Task SeedAdminAsync(
        IUserRepository users,
        IConfiguration configuration,
        ILogger logger)
    {
        if (await users.ExistsByEmailAsync(AdminEmail))
        {
            return;
        }

        var password = configuration["DevelopmentSeed:AdminPassword"];
        if (string.IsNullOrWhiteSpace(password))
        {
            logger.LogWarning(
                "No se creó el Admin de desarrollo. Configura DevelopmentSeed:AdminPassword para habilitarlo.");
            return;
        }

        var admin = await users.CreateAsync(new User
        {
            Nombre = AdminName,
            Email = AdminEmail,
            Password = BCrypt.Net.BCrypt.HashPassword(password),
            Rol = "Admin"
        });

        if (admin is null)
        {
            throw new InvalidOperationException("No se pudo crear el usuario Admin de desarrollo.");
        }
    }

    private static async Task<CategoriaEntity> EnsureCategoryAsync(
        ICategoriaRepository repository,
        string name)
    {
        var existing = await FindCategoryAsync(repository, name);
        if (existing is not null)
        {
            return existing;
        }

        var created = await repository.CreateAsync(new CategoriaEntity { Nombre = name });
        return created ?? throw new InvalidOperationException($"No se pudo crear la categoría de desarrollo '{name}'.");
    }

    private static async Task<CategoriaEntity?> FindCategoryAsync(
        ICategoriaRepository repository,
        string name)
    {
        for (var page = 1; ; page++)
        {
            var result = await repository.GetAllAsync(page, LookupPageSize);
            if (result.IsFailure)
            {
                throw new InvalidOperationException(result.Error.Message);
            }

            var found = result.Value.Objs.FirstOrDefault(category => category.Nombre == name);
            if (found is not null || page * LookupPageSize >= result.Value.Count)
            {
                return found;
            }
        }
    }

    private static async Task<ProveedorEntity> EnsureProviderAsync(
        IProveedorRepository repository,
        string name)
    {
        for (var page = 1; ; page++)
        {
            var result = await repository.GetAllAsync(page, LookupPageSize);
            if (result.IsFailure)
            {
                throw new InvalidOperationException(result.Error.Message);
            }

            var found = result.Value.Objs.FirstOrDefault(provider => provider.Nombre == name);
            if (found is not null)
            {
                return found;
            }

            if (page * LookupPageSize >= result.Value.Count)
            {
                var created = await repository.CreateAsync(new ProveedorEntity { Nombre = name });
                return created
                    ?? throw new InvalidOperationException($"No se pudo crear el proveedor de desarrollo '{name}'.");
            }
        }
    }

    private static async Task EnsureFunkoAsync(
        IFunkoRepository repository,
        string name,
        string categoryId,
        int providerId,
        decimal price,
        int stock)
    {
        if (await repository.ExistsByNameAsync(name))
        {
            return;
        }

        var created = await repository.CreateAsync(new FunkoEntity
        {
            Nombre = name,
            Imagen = string.Empty,
            Precio = price,
            Stock = stock,
            Categoria = categoryId,
            Proveedor = providerId
        });

        if (created is null)
        {
            throw new InvalidOperationException($"No se pudo crear el Funko de desarrollo '{name}'.");
        }
    }
}
