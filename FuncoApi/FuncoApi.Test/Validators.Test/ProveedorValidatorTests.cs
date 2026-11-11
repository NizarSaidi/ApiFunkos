using FuncoApi.Domain;
using FuncoApi.Models;
using FuncoApi.Validators;

namespace FuncoApi.Test.Validators.Test;

public class ProveedorValidatorTests
{
    [Fact]
    public void Validate_ReturnsError_WhenProviderIsNull()
    {
        var result = ProveedorValidator.Validate(null);

        AssertError(result, "El proveedor es obligatorio.");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Validate_ReturnsError_WhenIdIsNotPositive(int id)
    {
        var result = ProveedorValidator.Validate(new Proveedor { Id = id, Nombre = "Distribuidor" });

        AssertError(result, "El identificador del proveedor debe ser mayor que cero.");
    }

    [Theory]
    [InlineData("")]
    [InlineData("  ")]
    public void Validate_ReturnsError_WhenNameIsMissing(string nombre)
    {
        var result = ProveedorValidator.Validate(new Proveedor { Id = 1, Nombre = nombre });

        AssertError(result, "El nombre del proveedor es obligatorio.");
    }

    [Fact]
    public void Validate_ReturnsNull_WhenProviderIsValid()
    {
        var result = ProveedorValidator.Validate(new Proveedor { Id = 1, Nombre = "Distribuidor" });

        Assert.Null(result);
    }

    private static void AssertError(DomainError? error, string message)
    {
        Assert.NotNull(error);
        Assert.Equal(DomainErrorCode.Validation, error.Code);
        Assert.Equal(message, error.Message);
    }
}
