using FuncoApi.Entity;

namespace FuncoApi.Repositories;

/// <summary>Contrato de persistencia CRUD para entidades de Funko.</summary>
public interface IFunkoRepository: ICrudRepository<FunkoEntity,int>
{
    /// <summary>Comprueba si ya existe un Funko con el nombre indicado.</summary>
    /// <param name="name">Nombre que se quiere comprobar.</param>
    /// <param name="excludedId">ID que se excluye al comprobar una actualización.</param>
    Task<bool> ExistsByNameAsync(string name, int? excludedId = null);
}