using CSharpFunctionalExtensions;
using FuncoApi.Domain;
using FuncoApi.Entity;
using MongoDB.Driver;

namespace FuncoApi.Context.Mongo;

/// <summary>Implementa el CRUD de proveedores mediante la colección de MongoDB.</summary>
public class MongoProveedorDataSource : IProveedorDataSource
{
    private readonly MongoDbContext _context;
    private readonly IMongoCollection<ProveedorEntity> _collection;

    /// <summary>Obtiene la colección de proveedores desde el contexto Mongo.</summary>
    public MongoProveedorDataSource(MongoDbContext context)
    {
        _context = context;
        _collection = context.Proveedores;
    }

    /// <inheritdoc />
    public async Task<Result<(IReadOnlyList<ProveedorEntity> Objs, int Count), DomainError>> GetAllAsync(
        int page, int pageSize)
    {
        if (page <= 0 || pageSize <= 0)
        {
            return Result.Failure<(IReadOnlyList<ProveedorEntity> Objs, int Count), DomainError>(
                Errors.Validation("La página y el tamaño de página deben ser mayores que cero."));
        }

        var proveedores = await _collection.Find(_ => true)
            .Skip((page - 1) * pageSize)
            .Limit(pageSize)
            .ToListAsync();
        var count = (int)await _collection.CountDocumentsAsync(_ => true);

        return Result.Success<(IReadOnlyList<ProveedorEntity> Objs, int Count), DomainError>(
            (proveedores, count));
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
        obj.Id = await _context.GetNextProveedorIdAsync();
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
