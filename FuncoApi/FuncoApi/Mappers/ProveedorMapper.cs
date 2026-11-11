using FuncoApi.Entity;
using FuncoApi.Models;

namespace FuncoApi.Mappers;

/// <summary>Mapea proveedores entre los modelos de aplicación y las entidades MongoDB.</summary>
public static class ProveedorMapper
{
    /// <summary>Convierte una entidad persistida en el modelo de aplicación.</summary>
    public static Proveedor ToModel(ProveedorEntity entity) => new()
    {
        Id = entity.Id,
        Nombre = entity.Nombre
    };

    /// <summary>Convierte un modelo en la entidad persistible.</summary>
    public static ProveedorEntity ToEntity(Proveedor proveedor) => new()
    {
        Id = proveedor.Id,
        Nombre = proveedor.Nombre
    };
}
