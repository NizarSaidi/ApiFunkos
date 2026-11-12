using FuncoApi.Dto.ProveedorDto;
using FuncoApi.Entity;
using FuncoApi.Models;

namespace FuncoApi.Mappers;

/// <summary>Mapea proveedores entre los modelos de aplicación y las entidades MongoDB.</summary>
public static class ProveedorMapper
{
    /// <summary>Convierte los datos de creación en un modelo sin ID asignado.</summary>
    public static Proveedor ToModel(ProveedorCreateDto dto) => new()
    {
        Id = 0,
        Nombre = dto.Nombre
    };

    /// <summary>Convierte los datos de actualización en un modelo con el ID de la ruta.</summary>
    public static Proveedor ToModel(int id, ProveedorUpdateDto dto) => new()
    {
        Id = id,
        Nombre = dto.Nombre
    };

    /// <summary>Convierte el modelo de aplicación en el DTO de respuesta.</summary>
    public static ProveedorResponseDto ToResponseDto(Proveedor proveedor) => new()
    {
        Id = proveedor.Id,
        Nombre = proveedor.Nombre
    };

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
