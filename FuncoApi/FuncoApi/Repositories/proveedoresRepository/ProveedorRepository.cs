using FuncoApi.Context.Mongo;
using FuncoApi.Entity;

namespace FuncoApi.Repositories.proveedoresRepository;

public class ProveedorRepository : IProveedorRepository
{
    private readonly IProveedorDataSource _dataSource;

    public ProveedorRepository(IProveedorDataSource dataSource)
    {
        _dataSource = dataSource;
    }

    public async Task<IReadOnlyList<ProveedorEntity>> GetAllAsync()
    {
        return await _dataSource.GetAllAsync();
    }

    public async Task<ProveedorEntity?> GetByIdAsync(int id)
    {
        return await _dataSource.GetByIdAsync(id);
    }

    public async Task<ProveedorEntity?> CreateAsync(ProveedorEntity obj)
    {
        return await _dataSource.CreateAsync(obj);
    }

    public async Task<ProveedorEntity?> DeleteAsync(int id)
    {
        return await _dataSource.DeleteAsync(id);
    }

    public async Task<ProveedorEntity?> UpdateAsync(ProveedorEntity obj)
    {
        return await _dataSource.UpdateAsync(obj);
    }
}
