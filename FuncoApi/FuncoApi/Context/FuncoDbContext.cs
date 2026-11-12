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
            entity.Property(f => f.Precio).HasPrecision(18, 2);
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
        
        modelBuilder.Entity<UserEntity>(entity =>
        {
            entity.HasKey(u => u.Id);

            entity.Property(u => u.Nombre)
                .HasMaxLength(100)
                .IsRequired();

            entity.Property(u => u.Email)
                .HasMaxLength(255)
                .IsRequired();

            entity.Property(u => u.Password)
                .IsRequired();

            entity.Property(u => u.Rol)
                .HasMaxLength(20)
                .IsRequired();

            entity.HasIndex(u => u.Email)
                .IsUnique();

            entity.HasIndex(u => u.Nombre)
                .IsUnique();
        });
    }
    /// <summary>Conjunto relacional de Funkos activos.</summary>
    public virtual DbSet<FunkoEntity> Funkos => Set<FunkoEntity>();

    /// <summary>Conjunto relacional de categorías.</summary>
    public virtual DbSet<CategoriaEntity> Categorias => Set<CategoriaEntity>();

    /// <summary>Conjunto de usuarios definido por el modelo de persistencia.</summary>
    public virtual DbSet<UserEntity> Usuarios => Set<UserEntity>();

}
