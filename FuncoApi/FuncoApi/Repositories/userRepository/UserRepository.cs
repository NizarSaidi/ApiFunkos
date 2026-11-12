using FuncoApi.Context;
using FuncoApi.Entity;
using FuncoApi.Mappers;
using FuncoApi.Models;
using Microsoft.EntityFrameworkCore;

namespace FuncoApi.Repositories.userRepository;

/// <summary>Implementa la persistencia relacional de usuarios mediante Entity Framework Core.</summary>
public class UsuarioRepository : IUserRepository
{
    private readonly FuncoDbContext _context;

    /// <summary>Inicializa el repositorio con el contexto relacional de la aplicación.</summary>
    /// <param name="context">Contexto que permite consultar y guardar usuarios.</param>
    public UsuarioRepository(FuncoDbContext context)
    {
        _context = context;
    }

    /// <inheritdoc />
    public async Task<User?> GetByEmailAsync(string email)
    {
        var usuario = await _context.Usuarios.FirstOrDefaultAsync(u => u.Email == email && !u.IsDeleted);
        return usuario is null ? null : UserMapper.ToModel(usuario);
    }

    /// <inheritdoc />
    public async Task<bool> ExistsByNameAsync(string name)
    {
        return await _context.Usuarios.AnyAsync(u => u.Nombre == name);
    }

    /// <inheritdoc />
    public async Task<bool> ExistsByEmailAsync(string email)
    {
        return await _context.Usuarios.AnyAsync(u => u.Email == email);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<string>> GetAllEmailsAsync()
    {
        return await _context.Usuarios
            .Where(usuario => !usuario.IsDeleted && usuario.Email != "")
            .Select(usuario => usuario.Email)
            .ToListAsync();
    }

    /// <inheritdoc />
    public async Task<User?> GetByIdAsync(Guid id)
    {
        var usuario = await _context.Usuarios.FirstOrDefaultAsync(u => u.Id == id && !u.IsDeleted);
        return usuario is null ? null : UserMapper.ToModel(usuario);
    }

    /// <inheritdoc />
    public async Task<User?> CreateAsync(User usuario)
    {
        if (await ExistsByEmailAsync(usuario.Email))
        {
            return null;
        }

        if (await ExistsByNameAsync(usuario.Nombre))
        {
            return null;
        }
        
        var entity = UserMapper.ToEntity(usuario);
        entity.Id = Guid.Empty;
        entity.IsDeleted = false;
        entity.DeleteAt = null;
        entity.CreatedAt = DateTime.UtcNow;
        entity.UpdateAt = entity.CreatedAt;
        await _context.Usuarios.AddAsync(entity);

        await _context.SaveChangesAsync();
        
        return UserMapper.ToModel(entity);
    }

    /// <inheritdoc />
    public async Task<User?> UpdateAsync(User usuario)
    {
        var entity = await _context.Usuarios.FirstOrDefaultAsync(u => u.Id == usuario.Id && !u.IsDeleted);
        if (entity is null)
        {
            return null;
        }

        entity.Nombre = usuario.Nombre;
        entity.Email = usuario.Email;
        entity.Password = usuario.Password;
        entity.Rol = usuario.Rol;
        entity.UpdateAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return UserMapper.ToModel(entity);
    }

    /// <inheritdoc />
    public async Task<User?> DeleteAsync(string id)
    {
        var usuario = await _context.Usuarios.FirstOrDefaultAsync(u => u.Id == Guid.Parse(id) && !u.IsDeleted);
        if (usuario == null)
        {
            return null;
        }
        
        usuario.IsDeleted = true;
        usuario.DeleteAt = DateTime.UtcNow;
        usuario.UpdateAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return UserMapper.ToModel(usuario);
    }
}