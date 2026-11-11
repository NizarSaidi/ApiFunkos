using FuncoApi.Entity;

namespace FuncoApi.Context.Mongo;

public interface IProveedorDataSource
{
    Task<IReadOnlyList<ProveedorEntity>> GetAllAsync();
    Task<ProveedorEntity?> GetByIdAsync(int id);
    Task<ProveedorEntity?> CreateAsync(ProveedorEntity obj);
    Task<ProveedorEntity?> DeleteAsync(int id);
    Task<ProveedorEntity?> UpdateAsync(ProveedorEntity obj);
}

