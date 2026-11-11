using FuncoApi.Entity;
using MongoDB.Driver;

namespace FuncoApi.Context.Mongo;

/// <summary>Implementa el CRUD de proveedores mediante la colección de MongoDB.</summary>
public class MongoProveedorDataSource : IProveedorDataSource
{
    private readonly IMongoCollection<ProveedorEntity> _collection;

    /// <summary>Obtiene la colección de proveedores desde el contexto Mongo.</summary>
    public MongoProveedorDataSource(MongoDbContext context)
    {
        _collection = context.Proveedores;
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<ProveedorEntity>> GetAllAsync()
    {
        return await _collection.Find(_ => true).ToListAsync();
    }

    /// <inheritdoc />
    public Task<bool> ExistsByNameAsync(string name, int? excludedId = null)
    {
        var filter = Builders<ProveedorEntity>.Filter.Eq(provider => provider.Nombre, name);
        if (excludedId.HasValue)
        {
            filter &= Builders<ProveedorEntity>.Filter.Ne(provider => provider.Id, excludedId.Value);
        }

        return _collection.Find(filter).AnyAsync();
    }

    /// <inheritdoc />
    public async Task<ProveedorEntity?> GetByIdAsync(int id)
    {
        return await _collection.Find(p => p.Id == id).FirstOrDefaultAsync();
    }

    /// <inheritdoc />
    public async Task<ProveedorEntity?> CreateAsync(ProveedorEntity obj)
    {
        await _collection.InsertOneAsync(obj);
        return obj;
    }

    /// <inheritdoc />
    public async Task<ProveedorEntity?> DeleteAsync(int id)
    {
        var proveedor = await _collection.Find(p => p.Id == id).FirstOrDefaultAsync();
        if (proveedor is null)
        {
            return null;
        }

        await _collection.DeleteOneAsync(p => p.Id == id);
        return proveedor;
    }

    /// <inheritdoc />
    public async Task<ProveedorEntity?> UpdateAsync(ProveedorEntity obj)
    {
        var result = await _collection.ReplaceOneAsync(p => p.Id == obj.Id, obj);
        return result.MatchedCount == 0 ? null : obj;
    }
}
