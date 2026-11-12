using CSharpFunctionalExtensions;
using FuncoApi.Domain;
using FuncoApi.Models;

namespace FuncoApi.Services.objet;

/// <summary>Define operaciones de aplicación CRUD que devuelven un resultado de dominio.</summary>
/// <typeparam name="T">Tipo del modelo expuesto por el servicio.</typeparam>
/// <typeparam name="TId">Tipo del identificador del recurso.</typeparam>
public interface IService<T, TId>
{
    /// <summary>Obtiene una página de modelos junto con el número total disponible.</summary>
    /// <param name="page">Número de página a devolver; la primera es 1.</param>
    /// <returns>Un resultado con los modelos de la página y el total de registros.</returns>
    Task<Result<(IReadOnlyList<T> Objs, int Count), DomainError>> GetAll(int page = 1);

    /// <summary>Obtiene un modelo por identificador.</summary>
    /// <param name="id">Identificador del modelo.</param>
    /// <returns>Un resultado con el modelo o un error de dominio.</returns>
    Task<Result<T, DomainError>> GetById(TId id);

    /// <summary>Valida y crea un modelo.</summary>
    /// <param name="obj">Modelo que se va a crear.</param>
    /// <returns>Un resultado con el modelo creado o un error de dominio.</returns>
    Task<Result<T, DomainError>> Create(T obj);

    /// <summary>Valida y actualiza un modelo existente.</summary>
    /// <param name="id">Identificador de la ruta o de la operación.</param>
    /// <param name="obj">Modelo con los nuevos valores.</param>
    /// <returns>Un resultado con el modelo actualizado o un error de dominio.</returns>
    Task<Result<T, DomainError>> Update(TId id, T obj);

    /// <summary>Elimina el modelo identificado por el valor indicado.</summary>
    /// <param name="id">Identificador del modelo que se va a eliminar.</param>
    /// <returns>Un resultado con el modelo eliminado o un error de dominio.</returns>
    Task<Result<T, DomainError>> Delete(TId id);
}