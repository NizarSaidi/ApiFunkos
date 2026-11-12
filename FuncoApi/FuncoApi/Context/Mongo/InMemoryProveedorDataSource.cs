using CSharpFunctionalExtensions;
using FuncoApi.Domain;
using FuncoApi.Entity;

namespace FuncoApi.Context.Mongo;

/// <summary>Implementa el almacenamiento temporal de proveedores en memoria.</summary>
public sealed class InMemoryProveedorDataSource : IProveedorDataSource
{
    private readonly object _sync = new();
    private readonly Dictionary<int, ProveedorEntity> _proveedores = new();
    private int _nextId;

    /// <inheritdoc />
    public Task<Result<(IReadOnlyList<ProveedorEntity> Objs, int Count), DomainError>> GetAllAsync(
        int page, int pageSize)
    {
        if (page <= 0 || pageSize <= 0)
        {
            return Task.FromResult(
                Result.Failure<(IReadOnlyList<ProveedorEntity> Objs, int Count), DomainError>(
                    Errors.Validation("La página y el tamaño de página deben ser mayores que cero.")));
        }

        lock (_sync)
        {
            var result = _proveedores.Values
                .OrderBy(provider => provider.Id)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(Clone)
                .ToList();

            return Task.FromResult(
                Result.Success<(IReadOnlyList<ProveedorEntity> Objs, int Count), DomainError>(
                    (result, _proveedores.Count)));
        }
    }

    /// <inheritdoc />
    public Task<bool> ExistsByNameAsync(string name, int? excludedId = null)
    {
        lock (_sync)
        {
            return Task.FromResult(_proveedores.Values.Any(provider =>
                provider.Nombre == name && (!excludedId.HasValue || provider.Id != excludedId.Value)));
        }
    }

    /// <inheritdoc />
    public Task<ProveedorEntity?> GetByIdAsync(int id)
    {
        lock (_sync)
        {
            return Task.FromResult(
                _proveedores.TryGetValue(id, out var provider) ? Clone(provider) : null);
        }
    }

    /// <inheritdoc />
    public Task<ProveedorEntity?> CreateAsync(ProveedorEntity obj)
    {
        ArgumentNullException.ThrowIfNull(obj);

        lock (_sync)
        {
            if (_proveedores.Values.Any(provider => provider.Nombre == obj.Nombre))
            {
                return Task.FromResult<ProveedorEntity?>(null);
            }

            var now = DateTime.UtcNow;
            var provider = Clone(obj);
            provider.Id = ++_nextId;
            provider.CreatedAt = now;
            provider.UpdateAt = now;
            provider.DeleteAt = null;
            provider.IsDeleted = false;
            _proveedores.Add(provider.Id, provider);
            return Task.FromResult<ProveedorEntity?>(Clone(provider));
        }
    }

    /// <inheritdoc />
    public Task<ProveedorEntity?> DeleteAsync(int id)
    {
        lock (_sync)
        {
            if (!_proveedores.Remove(id, out var provider))
            {
                return Task.FromResult<ProveedorEntity?>(null);
            }

            return Task.FromResult<ProveedorEntity?>(Clone(provider));
        }
    }

    /// <inheritdoc />
    public Task<ProveedorEntity?> UpdateAsync(ProveedorEntity obj)
    {
        ArgumentNullException.ThrowIfNull(obj);

        lock (_sync)
        {
            if (!_proveedores.TryGetValue(obj.Id, out var current) ||
                _proveedores.Values.Any(provider => provider.Id != obj.Id && provider.Nombre == obj.Nombre))
            {
                return Task.FromResult<ProveedorEntity?>(null);
            }

            current.Nombre = obj.Nombre;
            current.UpdateAt = DateTime.UtcNow;
            return Task.FromResult<ProveedorEntity?>(Clone(current));
        }
    }

    private static ProveedorEntity Clone(ProveedorEntity provider) => new()
    {
        Id = provider.Id,
        Nombre = provider.Nombre,
        CreatedAt = provider.CreatedAt,
        UpdateAt = provider.UpdateAt,
        DeleteAt = provider.DeleteAt,
        IsDeleted = provider.IsDeleted
    };
}
