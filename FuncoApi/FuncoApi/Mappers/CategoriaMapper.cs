using FuncoApi.Dto.CategoriaDto;
using FuncoApi.Entity;
using FuncoApi.Models;

namespace FuncoApi.Mappers;

/// <summary>Mapea categorías entre los modelos de aplicación y las entidades persistidas.</summary>
public static class CategoriaMapper
{
    /// <summary>Convierte los datos de creación en un modelo sin ID asignado.</summary>
    public static Categoria ToModel(CategoriaCreateDto dto) => new()
    {
        Id = string.Empty,
        Nombre = dto.Nombre
    };

    /// <summary>Convierte los datos de actualización en un modelo con el ID de la ruta.</summary>
    public static Categoria ToModel(string id, CategoriaUpdateDto dto) => new()
    {
        Id = id,
        Nombre = dto.Nombre
    };

    /// <summary>Convierte el modelo de aplicación en el DTO de respuesta.</summary>
    public static CategoriaResponseDto ToResponseDto(Categoria categoria) => new()
    {
        Id = categoria.Id,
        Nombre = categoria.Nombre
    };

    /// <summary>Convierte una entidad persistida en el modelo de aplicación.</summary>
    public static Categoria ToModel(CategoriaEntity entity) => new()
    {
        Id = entity.Id,
        Nombre = entity.Nombre
    };

    /// <summary>Convierte un modelo en la entidad persistible.</summary>
    public static CategoriaEntity ToEntity(Categoria categoria) => new()
    {
        Id = categoria.Id,
        Nombre = categoria.Nombre
    };
}
