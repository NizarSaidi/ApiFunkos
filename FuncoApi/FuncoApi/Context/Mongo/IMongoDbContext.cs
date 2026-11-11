using FuncoApi.Entity;
using MongoDB.Driver;

namespace FuncoApi.Context.Mongo;

/// <summary>Expone las colecciones MongoDB requeridas por el acceso a proveedores.</summary>
public interface IMongoDbContext
{
    /// <summary>Colección MongoDB de proveedores.</summary>
    IMongoCollection<ProveedorEntity> Proveedores { get; }
}
