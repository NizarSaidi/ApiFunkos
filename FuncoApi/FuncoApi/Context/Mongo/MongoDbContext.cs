using FuncoApi.Entity;
using MongoDB.Bson;
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

    /// <summary>Obtiene atómicamente el siguiente identificador entero para un proveedor.</summary>
    public async Task<int> GetNextProveedorIdAsync()
    {
        var counters = _database.GetCollection<BsonDocument>("counters");
        var filter = Builders<BsonDocument>.Filter.Eq("_id", "proveedores");
        var highestExisting = await Proveedores.Find(_ => true)
            .SortByDescending(proveedor => proveedor.Id)
            .FirstOrDefaultAsync();
        await counters.UpdateOneAsync(
            filter,
            Builders<BsonDocument>.Update.Max("value", highestExisting?.Id ?? 0),
            new UpdateOptions { IsUpsert = true });

        var update = Builders<BsonDocument>.Update.Inc("value", 1);
        var options = new FindOneAndUpdateOptions<BsonDocument>
        {
            ReturnDocument = ReturnDocument.After
        };

        var counter = await counters.FindOneAndUpdateAsync(filter, update, options);
        return counter["value"].AsInt32;
    }
}