using CSharpFunctionalExtensions;
using FuncoApi.Context;
using FuncoApi.Domain;
using FuncoApi.Entity;
using Microsoft.EntityFrameworkCore;

namespace FuncoApi.Repositories;

/// <summary>Implementa el acceso EF Core a Funkos y su borrado lógico.</summary>
public class FunkoRepository : IFunkoRepository
{
    private readonly FuncoDbContext _context;

    /// <summary>Crea el repositorio para el contexto relacional indicado.</summary>
    public FunkoRepository(FuncoDbContext context) : base()
    {
        _context = context;
    }
    
    /// <inheritdoc />
    public async Task<Result<(IReadOnlyList<FunkoEntity> Objs, int Count), DomainError>> GetAllAsync(
        int page, int pageSize)
    {
        if (page <= 0 || pageSize <= 0)
        {
            return Result.Failure<(IReadOnlyList<FunkoEntity> Objs, int Count), DomainError>(
                Errors.Validation("La página y el tamaño de página deben ser mayores que cero."));
        }

        var funkos = await _context.Funkos
            .AsNoTracking()
            .Where(funko => !funko.IsDeleted)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
        var count = await _context.Funkos.CountAsync(funko => !funko.IsDeleted);

        return Result.Success<(IReadOnlyList<FunkoEntity> Objs, int Count), DomainError>(
            (funkos, count));
    }

    /// <inheritdoc />
    public Task<FunkoEntity?> GetByIdAsync(int id)
    {
        return _context.Funkos
            .AsNoTracking()
            .FirstOrDefaultAsync(funko => funko.Id == id && !funko.IsDeleted);
    }

    /// <inheritdoc />
    public Task<bool> ExistsByNameAsync(string name, int? excludedId = null)
    {
        return _context.Funkos.AnyAsync(funko =>
            !funko.IsDeleted &&
            funko.Nombre == name &&
            (!excludedId.HasValue || funko.Id != excludedId.Value));
    }

    /// <inheritdoc />
    public async Task<FunkoEntity?> CreateAsync(FunkoEntity obj)
    {
        obj.Id = 0;
        obj.IsDeleted = false;
        obj.DeleteAt = null;
        obj.CreatedAt = DateTime.UtcNow;
        obj.UpdateAt = obj.CreatedAt;

        await _context.Funkos.AddAsync(obj);
        await _context.SaveChangesAsync();
        return obj;
    }

    /// <summary>Marca un Funko activo como eliminado y actualiza sus marcas de tiempo.</summary>
    /// <inheritdoc />
    public async Task<FunkoEntity?> DeleteAsync(int id)
    {
        var funko = await _context.Funkos
            .FirstOrDefaultAsync(item => item.Id == id && !item.IsDeleted);
        if (funko is null)
        {
            return null;
        }

        var deletedAt = DateTime.UtcNow;
        funko.IsDeleted = true;
        funko.DeleteAt = deletedAt;
        funko.UpdateAt = deletedAt;
        await _context.SaveChangesAsync();
        return funko;
    }

    /// <inheritdoc />
    public async Task<FunkoEntity?> UpdateAsync(FunkoEntity obj)
    {
        var funko = await _context.Funkos
            .FirstOrDefaultAsync(item => item.Id == obj.Id && !item.IsDeleted);
        if (funko is null)
        {
            return null;
        }

        funko.Nombre = obj.Nombre;
        funko.Imagen = obj.Imagen;
        funko.Precio = obj.Precio;
        funko.Stock = obj.Stock;
        funko.Categoria = obj.Categoria;
        funko.Proveedor = obj.Proveedor;
        funko.UpdateAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return funko;
    }
}