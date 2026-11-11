using FuncoApi.Context.Mongo;
using FuncoApi.Entity;

namespace FuncoApi.Repositories.proveedoresRepository;

/// <summary>Implementa el repositorio de proveedores delegando en su fuente de datos MongoDB.</summary>
public class ProveedorRepository : IProveedorRepository
{
    private readonly IProveedorDataSource _dataSource;

    /// <summary>Crea el repositorio con la fuente de datos indicada.</summary>
    public ProveedorRepository(IProveedorDataSource dataSource)
    {
        _dataSource = dataSource;
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<ProveedorEntity>> GetAllAsync()
    {
        return await _dataSource.GetAllAsync();
    }

    /// <inheritdoc />
    public Task<bool> ExistsByNameAsync(string name, int? excludedId = null)
    {
        return _dataSource.ExistsByNameAsync(name, excludedId);
    }

    /// <inheritdoc />
    public async Task<ProveedorEntity?> GetByIdAsync(int id)
    {
        return await _dataSource.GetByIdAsync(id);
    }

    /// <inheritdoc />
    public async Task<ProveedorEntity?> CreateAsync(ProveedorEntity obj)
    {
        return await _dataSource.CreateAsync(obj);
    }

    /// <inheritdoc />
    public async Task<ProveedorEntity?> DeleteAsync(int id)
    {
        return await _dataSource.DeleteAsync(id);
    }

    /// <inheritdoc />
    public async Task<ProveedorEntity?> UpdateAsync(ProveedorEntity obj)
    {
        return await _dataSource.UpdateAsync(obj);
    }
}
