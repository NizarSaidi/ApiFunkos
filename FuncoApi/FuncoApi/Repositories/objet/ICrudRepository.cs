using System.Collections.Generic;

namespace FuncoApi.Repositories;

/// <summary>Define las operaciones asíncronas CRUD compartidas por los repositorios.</summary>
/// <typeparam name="T">Tipo de entidad administrada.</typeparam>
/// <typeparam name="TId">Tipo del identificador de la entidad.</typeparam>
public interface ICrudRepository<T,TId>
{
    /// <summary>Obtiene todas las entidades visibles para el repositorio.</summary>
    /// <returns>Una lista de solo lectura; puede estar vacía.</returns>
    Task<IReadOnlyList<T>> GetAllAsync();

    /// <summary>Busca una entidad por su identificador.</summary>
    /// <param name="id">Identificador de la entidad.</param>
    /// <returns>La entidad encontrada o <see langword="null"/> si no existe.</returns>
    Task<T?> GetByIdAsync(TId id);

    /// <summary>Persiste una entidad nueva.</summary>
    /// <param name="obj">Entidad que se va a crear.</param>
    /// <returns>La entidad creada o <see langword="null"/> si la operación no produjo resultado.</returns>
    Task<T?> CreateAsync(T obj);

    /// <summary>Elimina una entidad por su identificador.</summary>
    /// <param name="id">Identificador de la entidad que se va a eliminar.</param>
    /// <returns>La entidad eliminada o <see langword="null"/> si no existe.</returns>
    Task<T?> DeleteAsync(TId id);

    /// <summary>Actualiza una entidad existente.</summary>
    /// <param name="obj">Entidad con el identificador y los nuevos valores.</param>
    /// <returns>La entidad actualizada o <see langword="null"/> si no existe.</returns>
    Task<T?> UpdateAsync(T obj);
}