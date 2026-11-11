using FuncoApi.Context;
using FuncoApi.Entity;
using Microsoft.EntityFrameworkCore;

namespace FuncoApi.Repositories;

public class FunkoRepository : IFunkoRepository
{
    private readonly FuncoDbContext _context;

    public FunkoRepository(FuncoDbContext context) : base()
    {
        _context = context;
    }
    
    public async Task<IReadOnlyList<FunkoEntity>> GetAllAsync()
    {
        return await _context.Funkos
            .AsNoTracking()
            .Where(funko => !funko.IsDeleted)
            .ToListAsync();
    }

    public Task<FunkoEntity?> GetByIdAsync(int id)
    {
        return _context.Funkos
            .AsNoTracking()
            .FirstOrDefaultAsync(funko => funko.Id == id && !funko.IsDeleted);
    }

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