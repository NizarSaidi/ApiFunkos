using FuncoApi.Entity;
using MongoDB.Driver;

namespace FuncoApi.Context.Mongo;

public class MongoProveedorDataSource : IProveedorDataSource
{
    private readonly IMongoCollection<ProveedorEntity> _collection;

    public MongoProveedorDataSource(MongoDbContext context)
    {
        _collection = context.Proveedores;
    }

    public async Task<IReadOnlyList<ProveedorEntity>> GetAllAsync()
    {
        return await _collection.Find(_ => true).ToListAsync();
    }

    public async Task<ProveedorEntity?> GetByIdAsync(int id)
    {
        return await _collection.Find(p => p.Id == id).FirstOrDefaultAsync();
    }

    public async Task<ProveedorEntity?> CreateAsync(ProveedorEntity obj)
    {
        await _collection.InsertOneAsync(obj);
        return obj;
    }

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

    public async Task<ProveedorEntity?> UpdateAsync(ProveedorEntity obj)
    {
        var result = await _collection.ReplaceOneAsync(p => p.Id == obj.Id, obj);
        return result.MatchedCount == 0 ? null : obj;
    }
}

