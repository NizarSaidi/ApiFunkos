using FuncoApi.Entity;

namespace FuncoApi.Repositories;

/// <summary>Contrato de persistencia CRUD para categorías identificadas por texto.</summary>
public interface ICategoriaRepository : ICrudRepository<CategoriaEntity, string>
{
    /// <summary>Comprueba si ya existe una categoría con el nombre indicado.</summary>
    /// <param name="name">Nombre que se quiere comprobar.</param>
    /// <param name="excludedId">ID que se excluye al comprobar una actualización.</param>
    Task<bool> ExistsByNameAsync(string name, string? excludedId = null);
}
