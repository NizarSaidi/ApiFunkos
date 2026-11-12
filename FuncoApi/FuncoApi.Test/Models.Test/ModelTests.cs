using FuncoApi.Models;

namespace FuncoApi.Test.Models.Test;

/// <summary>Comprueba la asignación y los valores iniciales de los modelos de dominio.</summary>
public class ModelTests
{
    /// <summary>Comprueba que se pueden asignar y leer los datos de una categoría.</summary>
    [Fact]
    public void Categoria_ShouldAllowSettingAndReadingValues()
    {
        var categoria = new Categoria
        {
            Id = "anime",
            Nombre = "Anime"
        };

        Assert.Equal("anime", categoria.Id);
        Assert.Equal("Anime", categoria.Nombre);
    }

    /// <summary>Comprueba que se pueden asignar y leer los datos de un proveedor.</summary>
    [Fact]
    public void Proveedor_ShouldAllowSettingAndReadingValues()
    {
        var proveedor = new Proveedor
        {
            Id = 1,
            Nombre = "Funko"
        };

        Assert.Equal(1, proveedor.Id);
        Assert.Equal("Funko", proveedor.Nombre);
    }

    /// <summary>Comprueba el nombre inicial de un rol.</summary>
    [Fact]
    public void Rol_ShouldHaveDefaultValues()
    {
        var rol = new Rol();

        Assert.Equal(string.Empty, rol.Nombre);
    }

    /// <summary>Comprueba las propiedades del usuario y sus valores iniciales.</summary>
    [Fact]
    public void User_ShouldHaveExpectedPropertiesAndDefaultValues()
    {
        var user = new User
        {
            Id = Guid.NewGuid(),
            Nombre = "Ana",
            Email = "ana@example.com",
            Password = "secret",
            Rol = "Admin"
        };

        Assert.NotEqual(Guid.Empty, user.Id);
        Assert.Equal("Ana", user.Nombre);
        Assert.Equal("ana@example.com", user.Email);
        Assert.Equal("secret", user.Password);
        Assert.Equal("Admin", user.Rol);

        var emptyUser = new User();
        Assert.Equal(Guid.Empty, emptyUser.Id);
        Assert.Equal(string.Empty, emptyUser.Nombre);
        Assert.Equal(string.Empty, emptyUser.Email);
        Assert.Equal(string.Empty, emptyUser.Password);
        Assert.Equal(string.Empty, emptyUser.Rol);
    }
}
