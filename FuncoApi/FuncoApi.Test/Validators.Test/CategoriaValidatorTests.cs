using FuncoApi.Domain;
using FuncoApi.Models;
using FuncoApi.Validators;

namespace FuncoApi.Test.Validators.Test;

public class CategoriaValidatorTests
{
    [Fact]
    public void Validate_ReturnsError_WhenCategoryIsNull()
    {
        var result = CategoriaValidator.Validate(null);

        AssertError(result, "La categoría es obligatoria.");
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    public void Validate_ReturnsError_WhenIdIsMissing(string id)
    {
        var result = CategoriaValidator.Validate(new Categoria { Id = id, Nombre = "Anime" });

        AssertError(result, "El identificador de la categoría es obligatorio.");
    }

    [Theory]
    [InlineData("")]
    [InlineData("  ")]
    public void Validate_ReturnsError_WhenNameIsMissing(string nombre)
    {
        var result = CategoriaValidator.Validate(new Categoria { Id = "anime", Nombre = nombre });

        AssertError(result, "El nombre de la categoría es obligatorio.");
    }

    [Fact]
    public void Validate_ReturnsNull_WhenCategoryIsValid()
    {
        var result = CategoriaValidator.Validate(new Categoria { Id = "anime", Nombre = "Anime" });

        Assert.Null(result);
    }

    private static void AssertError(DomainError? error, string message)
    {
        Assert.NotNull(error);
        Assert.Equal(DomainErrorCode.Validation, error.Code);
        Assert.Equal(message, error.Message);
    }
}
