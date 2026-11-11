using FuncoApi.Domain;
using FuncoApi.Models;
using FuncoApi.Validators;

namespace FuncoApi.Test.Validators.Test;

public class FunkoValidatorTests
{
    [Fact]
    public void Validate_ReturnsValidationError_WhenFunkoIsNull()
    {
        var result = FunkoValidator.Validate(null);

        AssertError(result, "El Funko es obligatorio.");
    }

    [Theory]
    [InlineData("")]
    [InlineData("  ")]
    public void Validate_ReturnsValidationError_WhenNameIsMissing(string nombre)
    {
        var result = ValidateWith(funko => funko.Nombre = nombre);

        AssertError(result, "El nombre es obligatorio.");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(0.09)]
    public void Validate_ReturnsValidationError_WhenPriceIsBelowMinimum(decimal precio)
    {
        var result = ValidateWith(funko => funko.Precio = precio);

        AssertError(result, "El precio debe ser mayor o igual a 0,1.");
    }

    [Fact]
    public void Validate_ReturnsValidationError_WhenStockIsNegative()
    {
        var result = ValidateWith(funko => funko.Stock = -1);

        AssertError(result, "El stock no puede ser negativo.");
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    public void Validate_ReturnsValidationError_WhenCategoryIsMissing(string categoria)
    {
        var result = ValidateWith(funko => funko.Categoria = categoria);

        AssertError(result, "La categoría es obligatoria.");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Validate_ReturnsValidationError_WhenProviderIsNotPositive(int proveedor)
    {
        var result = ValidateWith(funko => funko.Proveedor = proveedor);

        AssertError(result, "El proveedor debe ser válido.");
    }

    [Fact]
    public void Validate_ReturnsNull_WhenValuesAreAtTheirMinimumBoundaries()
    {
        var funko = CreateValidFunko();
        funko.Precio = 0.1m;
        funko.Stock = 0;

        var result = FunkoValidator.Validate(funko);

        Assert.Null(result);
    }

    private static Funko CreateValidFunko() => new()
    {
        Nombre = "Funko",
        Imagen = "funko.png",
        Precio = 1,
        Stock = 2,
        Categoria = "Series",
        Proveedor = 1
    };

    private static DomainError? ValidateWith(Action<Funko> change)
    {
        var funko = CreateValidFunko();
        change(funko);
        return FunkoValidator.Validate(funko);
    }

    private static void AssertError(DomainError? error, string message)
    {
        Assert.NotNull(error);
        Assert.Equal(DomainErrorCode.Validation, error.Code);
        Assert.Equal(message, error.Message);
    }
}
