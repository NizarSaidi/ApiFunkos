namespace FuncoApi.Domain;

/// <summary>Centraliza los errores de dominio propios de las operaciones de usuario.</summary>
public static class UserErrors
{
    /// <summary>Crea un error cuando el nombre ya está registrado.</summary>
    public static DomainError DuplicateName(string name) =>
        new(DomainErrorCode.Conflict, $"Ya existe un usuario con nombre '{name}'.");

    /// <summary>Crea un error cuando el correo ya está registrado.</summary>
    public static DomainError DuplicateEmail(string email) =>
        new(DomainErrorCode.Conflict, $"Ya existe un usuario con correo '{email}'.");

    /// <summary>Crea un error cuando el usuario no existe.</summary>
    public static DomainError NotFound(string identifier) =>
        new(DomainErrorCode.NotFound, $"No se encontró un usuario con identificador '{identifier}'.");

    /// <summary>Crea un error para credenciales de autenticación inválidas.</summary>
    public static DomainError InvalidCredentials() =>
        new(DomainErrorCode.Unauthorized, "El correo o la contraseña no son válidos.");

    /// <summary>Crea un error cuando falla la persistencia de un usuario.</summary>
    public static DomainError PersistenceFailure() =>
        new(DomainErrorCode.PersistenceFailure, "No se pudo guardar el usuario.");
}
