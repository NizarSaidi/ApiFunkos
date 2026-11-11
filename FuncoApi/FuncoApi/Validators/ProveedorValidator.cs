using FuncoApi.Domain;
using FuncoApi.Models;

namespace FuncoApi.Validators;

/// <summary>Aplica las reglas de dominio a los modelos de proveedor.</summary>
public static class ProveedorValidator
{
    /// <summary>Comprueba que el proveedor tenga un ID positivo y un nombre.</summary>
    /// <param name="proveedor">Modelo que se va a validar; puede ser nulo.</param>
    /// <returns><see langword="null"/> si es válido; en caso contrario, el primer error encontrado.</returns>
    public static DomainError? Validate(Proveedor? proveedor)
    {
        if (proveedor is null)
        {
            return Errors.Validation("El proveedor es obligatorio.");
        }

        if (proveedor.Id <= 0)
        {
            return Errors.Validation("El identificador del proveedor debe ser mayor que cero.");
        }

        if (string.IsNullOrWhiteSpace(proveedor.Nombre))
        {
            return Errors.Validation("El nombre del proveedor es obligatorio.");
        }

        return null;
    }
}
