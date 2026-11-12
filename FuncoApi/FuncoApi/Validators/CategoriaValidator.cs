using FuncoApi.Domain;
using FuncoApi.Models;

namespace FuncoApi.Validators;

/// <summary>Aplica las reglas de dominio a los modelos de categoría.</summary>
public static class CategoriaValidator
{
    /// <summary>Comprueba los campos obligatorios y, si corresponde, el identificador de categoría.</summary>
    /// <param name="categoria">Modelo que se va a validar; puede ser nulo.</param>
    /// <param name="requireId">Indica si la operación requiere un ID ya asignado.</param>
    /// <returns><see langword="null"/> si es válido; en caso contrario, el primer error encontrado.</returns>
    public static DomainError? Validate(Categoria? categoria, bool requireId = true)
    {
        if (categoria is null)
        {
            return Errors.Validation("La categoría es obligatoria.");
        }

        if (requireId && string.IsNullOrWhiteSpace(categoria.Id))
        {
            return Errors.Validation("El identificador de la categoría es obligatorio.");
        }

        if (string.IsNullOrWhiteSpace(categoria.Nombre))
        {
            return Errors.Validation("El nombre de la categoría es obligatorio.");
        }

        return null;
    }
}
