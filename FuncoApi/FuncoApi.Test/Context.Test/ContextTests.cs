using FuncoApi.Context;
using FuncoApi.Context.Mongo;
using FuncoApi.Entity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace FuncoApi.Test.Context.Test;

/// <summary>Pruebas del modelo EF Core, contextos MongoDB y opciones de conexión.</summary>
public class FuncoDbContextTests
{
    /// <summary>Comprueba que el contexto relacional expone todos sus conjuntos.</summary>
    [Fact]
    public void FuncoDbContext_ShouldExposeConfiguredDbSets()
    {
        var options = new DbContextOptionsBuilder<FuncoDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        using var context = new FuncoDbContext(options);

        Assert.NotNull(context.Funkos);
        Assert.NotNull(context.Categorias);
        Assert.NotNull(context.Proveedores);
        Assert.NotNull(context.Usuarios);
        Assert.NotNull(context.Roles);
    }

    /// <summary>Comprueba que el modelo relacional crea índices para categoría y proveedor de Funko.</summary>
    [Fact]
    public void FuncoDbContext_OnModelCreating_ShouldConfigureFunkoEntityIndexes()
    {
        var options = new DbContextOptionsBuilder<FuncoDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        using var context = new FuncoDbContext(options);
        var entity = context.Model.FindEntityType(typeof(FunkoEntity));

        Assert.NotNull(entity);
        Assert.NotNull(entity!.GetIndexes().FirstOrDefault(i => i.Properties.Any(p => p.Name == nameof(FunkoEntity.Categoria))));
        Assert.NotNull(entity.GetIndexes().FirstOrDefault(i => i.Properties.Any(p => p.Name == nameof(FunkoEntity.Proveedor))));
    }
}

/// <summary>Comprueba la inicialización de MongoDB a partir de la configuración.</summary>
public class MongoDbContextTests
{
    /// <summary>Comprueba la base y la colección seleccionadas por el contexto Mongo.</summary>
    [Fact]
    public void MongoDbContext_ShouldCreateCollectionFromConfiguration()
    {
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["MongoSettings:ConnectionString"] = "mongodb://localhost:27017",
                ["MongoSettings:DatabaseName"] = "test_db"
            })
            .Build();

        var context = new MongoDbContext(config);

        Assert.NotNull(context.Proveedores);
        Assert.Equal("proveedores", context.Proveedores.CollectionNamespace.CollectionName);
        Assert.Equal("test_db", context.Proveedores.Database.DatabaseNamespace.DatabaseName);
    }

    /// <summary>Comprueba que la ausencia de cadena de conexión se comunica como configuración inválida.</summary>
    [Fact]
    public void MongoDbContext_ShouldThrow_WhenConnectionStringIsMissing()
    {
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["MongoSettings:DatabaseName"] = "test_db"
            })
            .Build();

        var exception = Assert.Throws<InvalidOperationException>(() => new MongoDbContext(config));
        Assert.Equal("Mongo connection string is not configured.", exception.Message);
    }

    /// <summary>Comprueba que la ausencia del nombre de base se comunica como configuración inválida.</summary>
    [Fact]
    public void MongoDbContext_ShouldThrow_WhenDatabaseNameIsMissing()
    {
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["MongoSettings:ConnectionString"] = "mongodb://localhost:27017"
            })
            .Build();

        var exception = Assert.Throws<InvalidOperationException>(() => new MongoDbContext(config));
        Assert.Equal("Mongo database name is not configured.", exception.Message);
    }
}

/// <summary>Comprueba el almacenamiento de los valores de configuración de MongoDB.</summary>
public class MongoSettingsTests
{
    /// <summary>Comprueba que las opciones conservan la cadena y el nombre de base asignados.</summary>
    [Fact]
    public void MongoSettings_ShouldStoreConnectionData()
    {
        var settings = new MongoSettings
        {
            ConnectionString = "mongodb://localhost:27017",
            DatabaseName = "test_db"
        };

        Assert.Equal("mongodb://localhost:27017", settings.ConnectionString);
        Assert.Equal("test_db", settings.DatabaseName);
    }
}
