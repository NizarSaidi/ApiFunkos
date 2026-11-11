using FuncoApi.Context;
using FuncoApi.Entity;
using Microsoft.EntityFrameworkCore;

namespace FuncoApi.Repositories;

/// <summary>Implementa las operaciones CRUD de categorías con EF Core.</summary>
public class CategoriaRepository : ICategoriaRepository
{
    private readonly FuncoDbContext _context;

    /// <summary>Crea el repositorio para el contexto relacional indicado.</summary>
    public CategoriaRepository(FuncoDbContext context)
    {
        _context = context;
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<CategoriaEntity>> GetAllAsync()
    {
        return await _context.Categorias
            .AsNoTracking()
            .ToListAsync();
    }

    /// <inheritdoc />
    public async Task<CategoriaEntity?> GetByIdAsync(string id)
    {
        return await _context.Categorias
            .AsNoTracking()
            .FirstOrDefaultAsync(categoria => categoria.Id == id);
    }

    /// <inheritdoc />
    public Task<bool> ExistsByNameAsync(string name, string? excludedId = null)
    {
        return _context.Categorias.AnyAsync(categoria =>
            categoria.Nombre == name &&
            (excludedId == null || categoria.Id != excludedId));
    }

    /// <inheritdoc />
    public async Task<CategoriaEntity?> CreateAsync(CategoriaEntity categoria)
    {
        await _context.Categorias.AddAsync(categoria);
        await _context.SaveChangesAsync();
        return categoria;
    }

    /// <inheritdoc />
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

    /// <inheritdoc />
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
