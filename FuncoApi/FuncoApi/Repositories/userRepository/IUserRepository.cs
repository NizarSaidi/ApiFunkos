using FuncoApi.Models;

namespace FuncoApi.Repositories.userRepository;

/// <summary>Define las operaciones de persistencia para los usuarios de la aplicación.</summary>
public interface IUserRepository
{
    /// <summary>Busca un usuario activo por su correo electrónico.</summary>
    /// <param name="email">Correo electrónico buscado.</param>
    /// <returns>El usuario encontrado o <see langword="null"/>.</returns>
    public Task<User?> GetByEmailAsync(string email);

    /// <summary>Indica si el nombre está registrado, incluso en usuarios borrados lógicamente.</summary>
    /// <param name="name">Nombre que se comprobará.</param>
    public Task<bool> ExistsByNameAsync(string name);

    /// <summary>Indica si el correo está registrado, incluso en usuarios borrados lógicamente.</summary>
    /// <param name="email">Correo que se comprobará.</param>
    public Task<bool> ExistsByEmailAsync(string email);

    /// <summary>Obtiene los correos de todos los usuarios activos.</summary>
    /// <returns>Lista de direcciones de usuarios no eliminados lógicamente.</returns>
    public Task<IReadOnlyList<string>> GetAllEmailsAsync();

    /// <summary>Busca un usuario activo por su identificador.</summary>
    /// <param name="id">Identificador del usuario.</param>
    /// <returns>El usuario encontrado o <see langword="null"/>.</returns>
    public Task<User?> GetByIdAsync(Guid id);

    /// <summary>Crea un usuario y asigna sus valores de persistencia.</summary>
    /// <param name="usuario">Modelo del usuario que se creará.</param>
    /// <returns>El usuario persistido o <see langword="null"/> si ya existe su nombre o correo.</returns>
    public Task<User?> CreateAsync(User usuario);
    
    /// <summary>Actualiza los datos de un usuario activo.</summary>
    /// <param name="usuario">Modelo con los datos actualizados.</param>
    /// <returns>El usuario actualizado o <see langword="null"/> si no existe.</returns>
    public Task<User?> UpdateAsync(User usuario);
    
    /// <summary>Marca como eliminado un usuario activo.</summary>
    /// <param name="id">Identificador del usuario, en formato GUID.</param>
    /// <returns>El usuario eliminado o <see langword="null"/> si no existe.</returns>
    public Task<User?> DeleteAsync(string id);
}