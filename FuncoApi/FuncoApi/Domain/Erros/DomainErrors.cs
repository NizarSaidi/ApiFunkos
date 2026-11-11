namespace FuncoApi.Domain;

/// <summary>Centraliza la creación de errores de dominio de la aplicación.</summary>
public static class Errors
{
    /// <summary>Crea un error de validación con el mensaje especificado.</summary>
    public static DomainError Validation(string message) =>
        new(DomainErrorCode.Validation, message);

    /// <summary>Crea un error para un recurso inexistente.</summary>
    /// <param name="resource">Nombre del recurso.</param>
    /// <param name="identifier">Identificador que no se encontró.</param>
    public static DomainError NotFound(string resource, string identifier) =>
        new(DomainErrorCode.NotFound, $"No se encontró {resource} con identificador '{identifier}'.");

    /// <summary>Crea un error de persistencia al guardar un recurso.</summary>
    /// <param name="resource">Nombre del recurso que no se pudo guardar.</param>
    public static DomainError PersistenceFailure(string resource) =>
        new(DomainErrorCode.PersistenceFailure, $"No se pudo crear {resource}.");

    /// <summary>Crea un error de conflicto cuando ya existe otro recurso con ese nombre.</summary>
    /// <param name="resource">Nombre del tipo de recurso.</param>
    /// <param name="name">Nombre duplicado.</param>
    public static DomainError DuplicateName(string resource, string name) =>
        new(DomainErrorCode.Conflict, $"Ya existe {resource} con nombre '{name}'.");
}
