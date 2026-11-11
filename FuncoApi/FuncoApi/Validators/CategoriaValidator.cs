using FuncoApi.Domain;
using FuncoApi.Models;

namespace FuncoApi.Validators;

/// <summary>Aplica las reglas de dominio a los modelos de categoría.</summary>
public static class CategoriaValidator
{
    /// <summary>Comprueba que la categoría tenga identificador y nombre.</summary>
    /// <param name="categoria">Modelo que se va a validar; puede ser nulo.</param>
    /// <returns><see langword="null"/> si es válido; en caso contrario, el primer error encontrado.</returns>
    public static DomainError? Validate(Categoria? categoria)
    {
        if (categoria is null)
        {
            return Errors.Validation("La categoría es obligatoria.");
        }

        if (string.IsNullOrWhiteSpace(categoria.Id))
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
