using FuncoApi.Entity;
using FuncoApi.Models;
using Microsoft.EntityFrameworkCore;

namespace FuncoApi.Context;

/// <summary>Contexto EF Core para las entidades relacionales de la API.</summary>
public class FuncoDbContext(DbContextOptions<FuncoDbContext> options) : DbContext(options)
{
    /// <summary>Configura claves, índices y el filtro global de borrado lógico de Funkos.</summary>
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

        modelBuilder.Entity<CategoriaEntity>(entity =>
        {
            entity.HasKey(categoria => categoria.Id);
            entity.Property(categoria => categoria.Id)
                .ValueGeneratedOnAdd()
                .HasDefaultValueSql("CONVERT(nvarchar(36), NEWID())");
        });
    }
    /// <summary>Conjunto relacional de Funkos activos.</summary>
    public virtual DbSet<FunkoEntity> Funkos => Set<FunkoEntity>();

    /// <summary>Conjunto relacional de categorías.</summary>
    public virtual DbSet<CategoriaEntity> Categorias => Set<CategoriaEntity>();

    /// <summary>Conjunto relacional de proveedores si se utiliza en el modelo SQL.</summary>
    public virtual DbSet<ProveedorEntity> Proveedores => Set<ProveedorEntity>();

    /// <summary>Conjunto de usuarios definido por el modelo de persistencia.</summary>
    public virtual DbSet<User> Usuarios => Set<User>();

    /// <summary>Conjunto de roles definido por el modelo de persistencia.</summary>
    public virtual DbSet<Rol> Roles => Set<Rol>();
}
