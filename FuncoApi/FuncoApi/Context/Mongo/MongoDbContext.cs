using FuncoApi.Entity;
using MongoDB.Driver;

namespace FuncoApi.Context.Mongo;

/// <summary>Crea el cliente MongoDB y expone la colección de proveedores configurada.</summary>
public class MongoDbContext : IMongoDbContext
{
    private readonly IMongoDatabase _database;

    /// <summary>Inicializa la conexión a partir de las claves <c>MongoSettings</c> de configuración.</summary>
    /// <param name="config">Configuración de la aplicación con la conexión y la base de datos.</param>
    /// <exception cref="InvalidOperationException">Falta la cadena de conexión o el nombre de la base.</exception>
    public MongoDbContext(IConfiguration config)
    {
        var connectionString = config["MongoSettings:ConnectionString"]
            ?? throw new InvalidOperationException("Mongo connection string is not configured.");
        var databaseName = config["MongoSettings:DatabaseName"]
            ?? throw new InvalidOperationException("Mongo database name is not configured.");

        var client = new MongoClient(connectionString);
        _database = client.GetDatabase(databaseName);
        Proveedores = _database.GetCollection<ProveedorEntity>("proveedores");
    }

    /// <inheritdoc />
    public IMongoCollection<ProveedorEntity> Proveedores { get; }
}