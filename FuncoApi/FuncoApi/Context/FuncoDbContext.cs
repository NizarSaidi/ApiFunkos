using FuncoApi.Entity;
using FuncoApi.Models;
using Microsoft.EntityFrameworkCore;

namespace FuncoApi.Context;

public class FuncoDbContext(DbContextOptions<FuncoDbContext> options) : DbContext(options)
{
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<FunkoEntity>(entity =>
        {
            entity.HasKey(f => f.Id);
            entity.Property(f => f.Id).ValueGeneratedOnAdd();
            entity.HasQueryFilter(f => !f.IsDeleted);

            entity.HasIndex(f => f.Categoria);

            entity.HasIndex(f => f.Proveedor);
        });
    }
    public virtual DbSet<FunkoEntity> Funkos => Set<FunkoEntity>();

    public virtual DbSet<CategoriaEntity> Categorias => Set<CategoriaEntity>();

    public virtual DbSet<ProveedorEntity> Proveedores => Set<ProveedorEntity>();

    public virtual DbSet<User> Usuarios => Set<User>();

    public virtual DbSet<Rol> Roles => Set<Rol>();
}
