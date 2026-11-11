using FuncoApi.Entity;
using MongoDB.Driver;

namespace FuncoApi.Context.Mongo;

public class MongoDbContext : IMongoDbContext
{
    private readonly IMongoDatabase _database;

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

    public IMongoCollection<ProveedorEntity> Proveedores { get; }
}