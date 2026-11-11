using FuncoApi.Context;
using FuncoApi.Entity;
using Microsoft.EntityFrameworkCore;

namespace FuncoApi.Repositories;

public class CategoriaRepository : ICategoriaRepository
{
    private readonly FuncoDbContext _context;

    public CategoriaRepository(FuncoDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<CategoriaEntity>> GetAllAsync()
    {
        return await _context.Categorias
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<CategoriaEntity?> GetByIdAsync(string id)
    {
        return await _context.Categorias
            .AsNoTracking()
            .FirstOrDefaultAsync(categoria => categoria.Id == id);
    }

    public async Task<CategoriaEntity?> CreateAsync(CategoriaEntity categoria)
    {
        await _context.Categorias.AddAsync(categoria);
        await _context.SaveChangesAsync();
        return categoria;
    }

    public async Task<CategoriaEntity?> DeleteAsync(string id)
    {
        var categoria = await _context.Categorias
            .FirstOrDefaultAsync(item => item.Id == id);
        if (categoria is null)
        {
            return null;
        }

        _context.Categorias.Remove(categoria);
        await _context.SaveChangesAsync();
        return categoria;
    }

    public async Task<CategoriaEntity?> UpdateAsync(CategoriaEntity categoria)
    {
        var existingCategoria = await _context.Categorias
            .FirstOrDefaultAsync(item => item.Id == categoria.Id);
        if (existingCategoria is null)
        {
            return null;
        }

        existingCategoria.Nombre = categoria.Nombre;
        await _context.SaveChangesAsync();
        return existingCategoria;
    }
}
