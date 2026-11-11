using FuncoApi.Domain;
using FuncoApi.Models;

namespace FuncoApi.Validators;

/// <summary>Aplica las reglas de dominio al modelo de un Funko.</summary>
public static class FunkoValidator
{
    /// <summary>Valida los campos obligatorios y los límites de precio, stock y proveedor.</summary>
    /// <param name="funko">Modelo que se va a validar; puede ser nulo.</param>
    /// <returns><see langword="null"/> si es válido; en caso contrario, el primer error encontrado.</returns>
    public static DomainError? Validate(Funko? funko)
    {
        if (funko is null)
        {
            return Errors.Validation("El Funko es obligatorio.");
        }

        if (string.IsNullOrWhiteSpace(funko.Nombre))
        {
            return Errors.Validation("El nombre es obligatorio.");
        }

        if (funko.Precio < 0.1m)
        {
            return Errors.Validation("El precio debe ser mayor o igual a 0,1.");
        }

        if (funko.Stock < 0)
        {
            return Errors.Validation("El stock no puede ser negativo.");
        }

        if (string.IsNullOrWhiteSpace(funko.Categoria))
        {
            return Errors.Validation("La categoría es obligatoria.");
        }

        if (funko.Proveedor <= 0)
        {
            return Errors.Validation("El proveedor debe ser válido.");
        }

        return null;
    }
}
