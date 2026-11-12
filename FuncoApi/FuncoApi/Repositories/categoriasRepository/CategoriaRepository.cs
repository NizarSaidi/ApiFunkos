using CSharpFunctionalExtensions;
using FuncoApi.Context;
using FuncoApi.Domain;
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
    public async Task<Result<(IReadOnlyList<CategoriaEntity> Objs, int Count), DomainError>> GetAllAsync(int page, int pageSize)
    {
        if (page <= 0 || pageSize <= 0)
        {
            return Result.Failure<(IReadOnlyList<CategoriaEntity> Objs, int Count), DomainError>(
                Errors.Validation("La página y el tamaño de página deben ser mayores que cero."));
        }

        var categorias = await _context.Categorias
            .AsNoTracking()
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
        var count = await _context.Categorias.CountAsync(categoria => !categoria.IsDeleted);

        return Result.Success<(IReadOnlyList<CategoriaEntity> Objs, int Count), DomainError>(
            (categorias, count));
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
        categoria.Id = null!;
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
