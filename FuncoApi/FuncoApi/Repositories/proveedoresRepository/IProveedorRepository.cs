using FuncoApi.Entity;

namespace FuncoApi.Repositories.proveedoresRepository;

/// <summary>Contrato de persistencia CRUD para proveedores identificados por entero.</summary>
public interface IProveedorRepository : ICrudRepository<ProveedorEntity, int>
{
    /// <summary>Comprueba si ya existe un proveedor con el nombre indicado.</summary>
    /// <param name="name">Nombre que se quiere comprobar.</param>
    /// <param name="excludedId">ID que se excluye al comprobar una actualización.</param>
    Task<bool> ExistsByNameAsync(string name, int? excludedId = null);
}
