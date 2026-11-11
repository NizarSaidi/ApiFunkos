using CSharpFunctionalExtensions;
using FuncoApi.Cache.objet;
using FuncoApi.Domain;
using FuncoApi.Mappers;
using FuncoApi.Models;
using FuncoApi.Repositories;
using FuncoApi.Validators;

namespace FuncoApi.Services.CategoriaService;

/// <summary>Coordina la validación, persistencia y caché de categorías.</summary>
public class CategoriaService : ICategoriaService
{
    private const string AllCategoriasCacheKey = "categorias:all";
    private static readonly TimeSpan CacheExpiration = TimeSpan.FromMinutes(5);

    private readonly ICacheService _cache;
    private readonly ICategoriaRepository _categoriaRepository;

    /// <summary>Inicializa el servicio con sus dependencias de caché y persistencia.</summary>
    /// <param name="cache">Caché para resultados de categorías.</param>
    /// <param name="categoriaRepository">Repositorio relacional de categorías.</param>
    public CategoriaService(ICacheService cache, ICategoriaRepository categoriaRepository)
    {
        _cache = cache;
        _categoriaRepository = categoriaRepository;
    }

    /// <summary>Obtiene todas las categorías, usando la caché si existe una lista vigente.</summary>
    /// <returns>Resultado con las categorías o un error de dominio.</returns>
    public async Task<Result<IReadOnlyList<Categoria>, DomainError>> GetAll()
    {
        var cachedCategorias = await _cache.GetAsync<List<Categoria>>(AllCategoriasCacheKey);
        if (cachedCategorias is not null)
        {
            return Result.Success<IReadOnlyList<Categoria>, DomainError>(cachedCategorias);
        }

        var categorias = (await _categoriaRepository.GetAllAsync())
            .Select(CategoriaMapper.ToModel)
            .ToList();

        await _cache.SetAsync(AllCategoriasCacheKey, categorias, CacheExpiration);
        return Result.Success<IReadOnlyList<Categoria>, DomainError>(categorias);
    }

    /// <summary>Busca una categoría por su identificador y guarda el resultado en caché.</summary>
    /// <param name="id">Identificador textual de la categoría.</param>
    /// <returns>La categoría encontrada o un error de validación o no encontrado.</returns>
    public async Task<Result<Categoria, DomainError>> GetById(string id)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            return Result.Failure<Categoria, DomainError>(
                Errors.Validation("El identificador de la categoría es obligatorio."));
        }

        var cacheKey = GetCategoriaCacheKey(id);
        var cachedCategoria = await _cache.GetAsync<Categoria>(cacheKey);
        if (cachedCategoria is not null)
        {
            return Result.Success<Categoria, DomainError>(cachedCategoria);
        }

        var entity = await _categoriaRepository.GetByIdAsync(id);
        if (entity is null)
        {
            return Result.Failure<Categoria, DomainError>(
                Errors.NotFound("la categoría", id));
        }

        var categoria = CategoriaMapper.ToModel(entity);
        await _cache.SetAsync(cacheKey, categoria, CacheExpiration);
        return Result.Success<Categoria, DomainError>(categoria);
    }

    /// <summary>Valida y crea una categoría, invalidando la caché de la lista.</summary>
    /// <param name="obj">Datos de la categoría que se va a crear.</param>
    /// <returns>La categoría creada o un error de validación o persistencia.</returns>
    public async Task<Result<Categoria, DomainError>> Create(Categoria obj)
    {
        var validationError = CategoriaValidator.Validate(obj);
        if (validationError is not null)
        {
            return Result.Failure<Categoria, DomainError>(validationError);
        }

        if (await _categoriaRepository.ExistsByNameAsync(obj.Nombre))
        {
            return Result.Failure<Categoria, DomainError>(
                Errors.DuplicateName("una categoría", obj.Nombre));
        }

        var created = await _categoriaRepository.CreateAsync(CategoriaMapper.ToEntity(obj));
        if (created is null)
        {
            return Result.Failure<Categoria, DomainError>(Errors.PersistenceFailure("la categoría"));
        }

        await _cache.RemoveAsync(AllCategoriasCacheKey);
        await _cache.RemoveAsync(GetCategoriaCacheKey(created.Id));
        return Result.Success<Categoria, DomainError>(CategoriaMapper.ToModel(created));
    }

    /// <summary>Valida y actualiza una categoría existente e invalida sus entradas de caché.</summary>
    /// <param name="id">Identificador esperado en la operación.</param>
    /// <param name="obj">Datos actualizados, incluido el mismo identificador.</param>
    /// <returns>La categoría actualizada o un error de dominio.</returns>
    public async Task<Result<Categoria, DomainError>> Update(string id, Categoria obj)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            return Result.Failure<Categoria, DomainError>(
                Errors.Validation("El identificador de la categoría es obligatorio."));
        }

        var validationError = CategoriaValidator.Validate(obj);
        if (validationError is not null)
        {
            return Result.Failure<Categoria, DomainError>(validationError);
        }

        if (!string.Equals(id, obj.Id, StringComparison.Ordinal))
        {
            return Result.Failure<Categoria, DomainError>(
                Errors.Validation("El identificador recibido no coincide con el de la categoría."));
        }

        if (await _categoriaRepository.ExistsByNameAsync(obj.Nombre, id))
        {
            return Result.Failure<Categoria, DomainError>(
                Errors.DuplicateName("una categoría", obj.Nombre));
        }

        var updated = await _categoriaRepository.UpdateAsync(CategoriaMapper.ToEntity(obj));
        if (updated is null)
        {
            return Result.Failure<Categoria, DomainError>(
                Errors.NotFound("la categoría", id));
        }

        await InvalidateCategoriaCache(id);
        return Result.Success<Categoria, DomainError>(CategoriaMapper.ToModel(updated));
    }

    /// <summary>Elimina una categoría existente e invalida sus entradas de caché.</summary>
    /// <param name="id">Identificador textual de la categoría.</param>
    /// <returns>La categoría eliminada o un error de validación o no encontrado.</returns>
    public async Task<Result<Categoria, DomainError>> Delete(string id)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            return Result.Failure<Categoria, DomainError>(
                Errors.Validation("El identificador de la categoría es obligatorio."));
        }

        var deleted = await _categoriaRepository.DeleteAsync(id);
        if (deleted is null)
        {
            return Result.Failure<Categoria, DomainError>(
                Errors.NotFound("la categoría", id));
        }

        await InvalidateCategoriaCache(id);
        return Result.Success<Categoria, DomainError>(CategoriaMapper.ToModel(deleted));
    }

    /// <summary>Invalida la lista y la entrada de caché individual de una categoría.</summary>
    private async Task InvalidateCategoriaCache(string id)
    {
        await _cache.RemoveAsync(AllCategoriasCacheKey);
        await _cache.RemoveAsync(GetCategoriaCacheKey(id));
    }

    /// <summary>Construye la clave de caché individual de una categoría.</summary>
    private static string GetCategoriaCacheKey(string id) => $"categorias:{id}";

}
