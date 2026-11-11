using System.Collections.Generic;

namespace FuncoApi.Repositories;

public interface ICrudRepository<T,TId>
{
    Task<IReadOnlyList<T>> GetAllAsync();
    Task<T?> GetByIdAsync(TId id);
    Task<T?> CreateAsync(T obj);
    Task<T?> DeleteAsync(TId id);
    Task<T?> UpdateAsync(T obj);
}