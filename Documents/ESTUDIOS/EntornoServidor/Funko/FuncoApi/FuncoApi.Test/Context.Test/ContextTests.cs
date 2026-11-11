using FuncoApi.Context;
using FuncoApi.Context.Mongo;
using FuncoApi.Entity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace FuncoApi.Test.Context.Test;

public class FuncoDbContextTests
{
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

public class MongoDbContextTests
{
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

public class MongoSettingsTests
{
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

