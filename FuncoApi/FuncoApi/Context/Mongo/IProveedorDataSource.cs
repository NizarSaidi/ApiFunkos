using CSharpFunctionalExtensions;
using FuncoApi.Domain;
using FuncoApi.Entity;

namespace FuncoApi.Context.Mongo;

/// <summary>Define las operaciones de acceso a documentos de proveedores en MongoDB.</summary>
public interface IProveedorDataSource
{
    /// <summary>Obtiene una página de proveedores y el total de documentos almacenados.</summary>
    /// <param name="page">Número de página; la primera página es 1.</param>
    /// <param name="pageSize">Número máximo de proveedores que devuelve la página.</param>
    /// <returns>La página y el total, o un error de dominio si falla la consulta.</returns>
    Task<Result<(IReadOnlyList<ProveedorEntity> Objs, int Count), DomainError>> GetAllAsync(
        int page, int pageSize);

    /// <summary>Comprueba si ya existe un proveedor con el nombre indicado.</summary>
    /// <param name="name">Nombre que se quiere comprobar.</param>
    /// <param name="excludedId">ID que se excluye al comprobar una actualización.</param>
    Task<bool> ExistsByNameAsync(string name, int? excludedId = null);

    /// <summary>Busca un proveedor por identificador.</summary>
    /// <param name="id">Identificador del proveedor.</param>
    /// <returns>El proveedor encontrado o <see langword="null"/>.</returns>
    Task<ProveedorEntity?> GetByIdAsync(int id);

    /// <summary>Inserta un proveedor.</summary>
    /// <param name="obj">Documento que se va a insertar.</param>
    /// <returns>El documento insertado.</returns>
    Task<ProveedorEntity?> CreateAsync(ProveedorEntity obj);

    /// <summary>Elimina un proveedor por identificador.</summary>
    /// <param name="id">Identificador del proveedor que se va a eliminar.</param>
    /// <returns>El proveedor eliminado o <see langword="null"/>.</returns>
    Task<ProveedorEntity?> DeleteAsync(int id);

    /// <summary>Reemplaza un proveedor existente.</summary>
    /// <param name="obj">Documento con el identificador y los nuevos valores.</param>
    /// <returns>El documento actualizado o <see langword="null"/> si no existe.</returns>
    Task<ProveedorEntity?> UpdateAsync(ProveedorEntity obj);
}
