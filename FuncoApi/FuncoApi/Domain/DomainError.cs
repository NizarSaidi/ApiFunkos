namespace FuncoApi.Domain;

/// <summary>Clasifica los errores de dominio que puede devolver un servicio.</summary>
public enum DomainErrorCode
{
    /// <summary>Los datos recibidos no cumplen las reglas de validación.</summary>
    Validation,
    /// <summary>No existe el recurso solicitado.</summary>
    NotFound,
    /// <summary>La operación entra en conflicto con el estado existente.</summary>
    Conflict,
    /// <summary>La autenticación no pudo completarse por credenciales inválidas.</summary>
    Unauthorized,
    /// <summary>La persistencia no pudo completar la operación esperada.</summary>
    PersistenceFailure
}

/// <summary>Describe un error de dominio con su categoría y mensaje legible.</summary>
/// <param name="Code">Categoría del error.</param>
/// <param name="Message">Descripción destinada al consumidor del servicio.</param>
public sealed record DomainError(DomainErrorCode Code, string Message);
