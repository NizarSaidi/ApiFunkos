using FuncoApi.Context;
using FuncoApi.Entity;
using FuncoApi.Mappers;
using FuncoApi.Models;
using Microsoft.EntityFrameworkCore;

namespace FuncoApi.Repositories.userRepository;

public class UsuarioRepository : IUserRepository
{
    private readonly FuncoDbContext _context;

    public UsuarioRepository(FuncoDbContext context)
    {
        _context = context;
    }

    public async Task<UserEntity?> GetByEmailAsync(string email)
    {
        return await _context.Usuarios.FirstOrDefaultAsync(u => u.Email == email);
    }

    public async Task<UserEntity?> GetByIdAsync(Guid id)
    {
        return await _context.Usuarios.FirstOrDefaultAsync(u => u.Id == id);
    }

    public async Task<UserEntity> CreateAsync(UserEntity usuario)
    {
        
        usuario.Id = Guid.Empty;
        usuario.IsDeleted = false; 
        usuario.DeleteAt = null;
        usuario.CreatedAt = DateTime.UtcNow; 
        usuario.UpdateAt = usuario.CreatedAt;
        await _context.Usuarios.AddAsync(usuario);

        await _context.SaveChangesAsync();
        
        return usuario;
    }

    public Task<UserEntity> UpdateAsync(UserEntity usuario)
    {
        usuario.UpdateAt = DateTime.UtcNow;
        _context.Usuarios.Update(usuario);

        _context.SaveChanges();

        return Task.FromResult(usuario);
    }

    public Task DeleteAsync(string id)
    {
        var usuario = _context.Usuarios.FirstOrDefault(u => u.Id == Guid.Parse(id));
        if (usuario == null)
        {
            throw new InvalidOperationException("User not found");
        }
        
        usuario.IsDeleted = true;
        usuario.DeleteAt = DateTime.UtcNow;
        usuario.UpdateAt = DateTime.UtcNow;
        _context.Usuarios.Update(usuario);
        _context.SaveChanges();

        return Task.CompletedTask;
    }
}