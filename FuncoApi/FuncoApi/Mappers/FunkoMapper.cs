using FuncoApi.Entity;
using FuncoApi.Models;

namespace FuncoApi.Mappers;

/// <summary>Mapea Funkos entre los modelos de aplicación y las entidades persistidas.</summary>
public static class FunkoMapper
{
    /// <summary>Convierte una entidad persistida en el modelo de API.</summary>
    public static Funko ToModel(FunkoEntity entity) => new()
    {
        Id = entity.Id,
        Nombre = entity.Nombre,
        Imagen = entity.Imagen,
        Precio = entity.Precio,
        Stock = entity.Stock,
        Categoria = entity.Categoria,
        Proveedor = entity.Proveedor
    };

    /// <summary>Convierte un modelo de API en una entidad persistible.</summary>
    public static FunkoEntity ToEntity(Funko funko) => new()
    {
        Id = funko.Id,
        Nombre = funko.Nombre,
        Imagen = funko.Imagen,
        Precio = funko.Precio,
        Stock = funko.Stock,
        Categoria = funko.Categoria,
        Proveedor = funko.Proveedor
    };
}
