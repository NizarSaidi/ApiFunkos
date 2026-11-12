using CSharpFunctionalExtensions;
using FuncoApi.Cache.objet;
using FuncoApi.Configure;
using FuncoApi.Domain;
using FuncoApi.Mappers;
using FuncoApi.Models;
using FuncoApi.Repositories.proveedoresRepository;
using FuncoApi.Validators;

namespace FuncoApi.Services.ProveedoresService;

/// <summary>Coordina validación, persistencia MongoDB y caché de proveedores.</summary>
public class ProveedorService : IProveedorService
{
    private const string AllProveedoresCacheKey = "proveedores:all";
    private static readonly TimeSpan CacheExpiration = TimeSpan.FromMinutes(5);

    private readonly ICacheService _cache;
    private readonly IProveedorRepository _proveedorRepository;

    /// <summary>Inicializa el servicio con sus dependencias de caché y persistencia.</summary>
    /// <param name="cache">Caché para resultados de proveedores.</param>
    /// <param name="proveedorRepository">Repositorio MongoDB de proveedores.</param>
    public ProveedorService(ICacheService cache, IProveedorRepository proveedorRepository)
    {
        _cache = cache;
        _proveedorRepository = proveedorRepository;
    }

    /// <summary>Obtiene una página de proveedores y el total, o devuelve errores de validación o consulta.</summary>
    /// <param name="page">Número de página, empezando por 1.</param>
    /// <returns>Resultado con los proveedores de la página y el total de proveedores.</returns>
    public async Task<Result<(IReadOnlyList<Proveedor> Objs, int Count), DomainError>> GetAll(int page = 1)
    {
        if (page <= 0)
        {
            return Result.Failure<(IReadOnlyList<Proveedor> Objs, int Count), DomainError>(
                Errors.Validation("El número de página debe ser mayor que cero."));
        }

        var result = await _proveedorRepository.GetAllAsync(page, ConfigurationApp.PageSize);
        if (result.IsFailure)
        {
            return Result.Failure<(IReadOnlyList<Proveedor> Objs, int Count), DomainError>(result.Error);
        }

        var proveedores = result.Value.Objs
            .Select(ProveedorMapper.ToModel)
            .ToList();

        return Result.Success<(IReadOnlyList<Proveedor> Objs, int Count), DomainError>(
            (proveedores, result.Value.Count));
    }

    /// <summary>Busca un proveedor por su identificador y guarda el resultado en caché.</summary>
    /// <param name="id">Identificador positivo del proveedor.</param>
    /// <returns>El proveedor encontrado o un error de validación o no encontrado.</returns>
    public async Task<Result<Proveedor, DomainError>> GetById(int id)
    {
        if (id <= 0)
        {
            return Result.Failure<Proveedor, DomainError>(
                Errors.Validation("El identificador del proveedor debe ser mayor que cero."));
        }

        var cacheKey = GetProveedorCacheKey(id);
        var cachedProveedor = await _cache.GetAsync<Proveedor>(cacheKey);
        if (cachedProveedor is not null)
        {
            return Result.Success<Proveedor, DomainError>(cachedProveedor);
        }

        var entity = await _proveedorRepository.GetByIdAsync(id);
        if (entity is null)
        {
            return Result.Failure<Proveedor, DomainError>(
                Errors.NotFound("el proveedor", id.ToString()));
        }

        var proveedor = ProveedorMapper.ToModel(entity);
        await _cache.SetAsync(cacheKey, proveedor, CacheExpiration);
        return Result.Success<Proveedor, DomainError>(proveedor);
    }

    /// <summary>Valida y crea un proveedor, invalidando la caché de la lista.</summary>
    /// <param name="obj">Datos del proveedor que se va a crear.</param>
    /// <returns>El proveedor creado o un error de validación o persistencia.</returns>
    public async Task<Result<Proveedor, DomainError>> Create(Proveedor obj)
    {
        var validationError = ProveedorValidator.Validate(obj, requireId: false);
        if (validationError is not null)
        {
            return Result.Failure<Proveedor, DomainError>(validationError);
        }

        if (await _proveedorRepository.ExistsByNameAsync(obj.Nombre))
        {
            return Result.Failure<Proveedor, DomainError>(
                Errors.DuplicateName("un proveedor", obj.Nombre));
        }

        var created = await _proveedorRepository.CreateAsync(ProveedorMapper.ToEntity(obj));
        if (created is null)
        {
            return Result.Failure<Proveedor, DomainError>(Errors.PersistenceFailure("el proveedor"));
        }

        await _cache.RemoveAsync(AllProveedoresCacheKey);
        await _cache.RemoveAsync(GetProveedorCacheKey(created.Id));
        return Result.Success<Proveedor, DomainError>(ProveedorMapper.ToModel(created));
    }

    /// <summary>Valida y actualiza un proveedor existente e invalida sus entradas de caché.</summary>
    /// <param name="id">Identificador esperado en la operación.</param>
    /// <param name="obj">Datos actualizados, incluido el mismo identificador.</param>
    /// <returns>El proveedor actualizado o un error de dominio.</returns>
    public async Task<Result<Proveedor, DomainError>> Update(int id, Proveedor obj)
    {
        if (id <= 0)
        {
            return Result.Failure<Proveedor, DomainError>(
                Errors.Validation("El identificador del proveedor debe ser mayor que cero."));
        }

        var validationError = ProveedorValidator.Validate(obj);
        if (validationError is not null)
        {
            return Result.Failure<Proveedor, DomainError>(validationError);
        }

        if (obj.Id != id)
        {
            return Result.Failure<Proveedor, DomainError>(
                Errors.Validation("El identificador recibido no coincide con el del proveedor."));
        }

        if (await _proveedorRepository.ExistsByNameAsync(obj.Nombre, id))
        {
            return Result.Failure<Proveedor, DomainError>(
                Errors.DuplicateName("un proveedor", obj.Nombre));
        }

        var updated = await _proveedorRepository.UpdateAsync(ProveedorMapper.ToEntity(obj));
        if (updated is null)
        {
            return Result.Failure<Proveedor, DomainError>(
                Errors.NotFound("el proveedor", id.ToString()));
        }

        await InvalidateProveedorCache(id);
        return Result.Success<Proveedor, DomainError>(ProveedorMapper.ToModel(updated));
    }

    /// <summary>Elimina un proveedor existente e invalida sus entradas de caché.</summary>
    /// <param name="id">Identificador positivo del proveedor.</param>
    /// <returns>El proveedor eliminado o un error de validación o no encontrado.</returns>
    public async Task<Result<Proveedor, DomainError>> Delete(int id)
    {
        if (id <= 0)
        {
            return Result.Failure<Proveedor, DomainError>(
                Errors.Validation("El identificador del proveedor debe ser mayor que cero."));
        }

        var deleted = await _proveedorRepository.DeleteAsync(id);
        if (deleted is null)
        {
            return Result.Failure<Proveedor, DomainError>(
                Errors.NotFound("el proveedor", id.ToString()));
        }

        await InvalidateProveedorCache(id);
        return Result.Success<Proveedor, DomainError>(ProveedorMapper.ToModel(deleted));
    }

    /// <summary>Invalida la lista y la entrada de caché individual del proveedor.</summary>
    private async Task InvalidateProveedorCache(int id)
    {
        await _cache.RemoveAsync(AllProveedoresCacheKey);
        await _cache.RemoveAsync(GetProveedorCacheKey(id));
    }

    /// <summary>Construye la clave de caché individual de un proveedor.</summary>
    private static string GetProveedorCacheKey(int id) => $"proveedores:{id}";

}
