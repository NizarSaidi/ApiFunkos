using FuncoApi.Entity;
using MongoDB.Driver;

namespace FuncoApi.Context.Mongo;

public interface IMongoDbContext
{
    IMongoCollection<ProveedorEntity> Proveedores { get; }
}

