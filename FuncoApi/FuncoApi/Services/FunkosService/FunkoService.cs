using CSharpFunctionalExtensions;
using FuncoApi.Cache.objet;
using FuncoApi.Domain;
using FuncoApi.Mappers;
using FuncoApi.Models;
using FuncoApi.Repositories;
using FuncoApi.Validators;

namespace FuncoApi.Services.FunkosService;

/// <summary>Coordina la validación, persistencia y caché de Funkos.</summary>
public class FunkoService : IFunkosService
{
    private const string AllFunkosCacheKey = "funkos:all";
    private static readonly TimeSpan CacheExpiration = TimeSpan.FromMinutes(5);

    private readonly ICacheService _cache;
    private readonly IFunkoRepository _funkoRepository;

    /// <summary>Inicializa el servicio con sus dependencias de caché y persistencia.</summary>
    /// <param name="cache">Caché para resultados de Funkos.</param>
    /// <param name="funkoRepository">Repositorio de persistencia relacional.</param>
    public FunkoService(ICacheService cache, IFunkoRepository funkoRepository)
    {
        _cache = cache;
        _funkoRepository = funkoRepository;
    }

    /// <summary>Obtiene todos los Funkos activos, usando la caché si hay una lista disponible.</summary>
    /// <returns>Resultado con la lista de modelos.</returns>
    public async Task<Result<IReadOnlyList<Funko>, DomainError>> GetAll()
    {
        var funkos = (await _funkoRepository.GetAllAsync())
            .Select(FunkoMapper.ToModel)
            .ToList();

        await _cache.SetAsync(AllFunkosCacheKey, funkos, CacheExpiration);
        return Result.Success<IReadOnlyList<Funko>, DomainError>(funkos);
    }

    /// <summary>Obtiene un Funko activo por ID y guarda el resultado en caché.</summary>
    /// <param name="id">Identificador positivo del Funko.</param>
    /// <returns>El Funko o un error de validación o de recurso no encontrado.</returns>
    public async Task<Result<Funko, DomainError>> GetById(int id)
    {
        if (id <= 0)
        {
            return Result.Failure<Funko, DomainError>(
                Errors.Validation("El identificador debe ser mayor que cero."));
        }

        var cacheKey = GetFunkoCacheKey(id);
        var cachedFunko = await _cache.GetAsync<Funko>(cacheKey);
        if (cachedFunko is not null)
        {
            return Result.Success<Funko, DomainError>(cachedFunko);
        }

        var entity = await _funkoRepository.GetByIdAsync(id);
        if (entity is null)
        {
            return Result.Failure<Funko, DomainError>(
                Errors.NotFound("el Funko", id.ToString()));
        }

        var funko = FunkoMapper.ToModel(entity);
        await _cache.SetAsync(cacheKey, funko, CacheExpiration);
        return Result.Success<Funko, DomainError>(funko);
    }

    /// <summary>Valida y persiste un Funko nuevo, invalidando la caché de la lista.</summary>
    /// <param name="obj">Datos del Funko que se va a crear.</param>
    /// <returns>El Funko creado o un error de validación o persistencia.</returns>
    public async Task<Result<Funko, DomainError>> Create(Funko obj)
    {
        var validationError = FunkoValidator.Validate(obj);
        if (validationError is not null)
        {
            return Result.Failure<Funko, DomainError>(validationError);
        }

        if (await _funkoRepository.ExistsByNameAsync(obj.Nombre))
        {
            return Result.Failure<Funko, DomainError>(
                Errors.DuplicateName("un Funko", obj.Nombre));
        }

        var created = await _funkoRepository.CreateAsync(FunkoMapper.ToEntity(obj));
        if (created is null)
        {
            return Result.Failure<Funko, DomainError>(Errors.PersistenceFailure("el Funko"));
        }

        await _cache.RemoveAsync(AllFunkosCacheKey);
        return Result.Success<Funko, DomainError>(FunkoMapper.ToModel(created));
    }

    /// <summary>Valida y actualiza un Funko existente, invalidando sus entradas de caché.</summary>
    /// <param name="id">Identificador esperado en la ruta.</param>
    /// <param name="obj">Datos actualizados, incluido el mismo identificador.</param>
    /// <returns>El Funko actualizado o un error de validación o de recurso no encontrado.</returns>
    public async Task<Result<Funko, DomainError>> Update(int id, Funko obj)
    {
        if (id <= 0)
        {
            return Result.Failure<Funko, DomainError>(
                Errors.Validation("El identificador debe ser mayor que cero."));
        }

        var validationError = FunkoValidator.Validate(obj);
        if (validationError is not null)
        {
            return Result.Failure<Funko, DomainError>(validationError);
        }

        if (obj.Id != id)
        {
            return Result.Failure<Funko, DomainError>(
                Errors.Validation("El identificador de la ruta no coincide con el del Funko."));
        }

        if (await _funkoRepository.ExistsByNameAsync(obj.Nombre, id))
        {
            return Result.Failure<Funko, DomainError>(
                Errors.DuplicateName("un Funko", obj.Nombre));
        }

        var updated = await _funkoRepository.UpdateAsync(FunkoMapper.ToEntity(obj));
        if (updated is null)
        {
            return Result.Failure<Funko, DomainError>(
                Errors.NotFound("el Funko", id.ToString()));
        }

        await InvalidateFunkoCache(id);
        return Result.Success<Funko, DomainError>(FunkoMapper.ToModel(updated));
    }

    /// <summary>Marca como eliminado un Funko y elimina sus entradas de caché.</summary>
    /// <param name="id">Identificador positivo del Funko.</param>
    /// <returns>El Funko eliminado o un error de validación o de recurso no encontrado.</returns>
    public async Task<Result<Funko, DomainError>> Delete(int id)
    {
        if (id <= 0)
        {
            return Result.Failure<Funko, DomainError>(
                Errors.Validation("El identificador debe ser mayor que cero."));
        }

        var deleted = await _funkoRepository.DeleteAsync(id);
        if (deleted is null)
        {
            return Result.Failure<Funko, DomainError>(
                Errors.NotFound("el Funko", id.ToString()));
        }

        await InvalidateFunkoCache(id);
        return Result.Success<Funko, DomainError>(FunkoMapper.ToModel(deleted));
    }

    /// <summary>Invalida la lista y la entrada de caché del Funko indicado.</summary>
    private async Task InvalidateFunkoCache(int id)
    {
        await _cache.RemoveAsync(AllFunkosCacheKey);
        await _cache.RemoveAsync(GetFunkoCacheKey(id));
    }

    /// <summary>Construye la clave de caché individual de un Funko.</summary>
    private static string GetFunkoCacheKey(int id) => $"funkos:{id}";

}