using System.ComponentModel.DataAnnotations;

namespace FuncoApi.Dto.CategoriaDto;

/// <summary>Datos necesarios para crear una categoría.</summary>
public class CategoriaCreateDto
{
    /// <summary>Nombre único de la categoría.</summary>
    [Required]
    public string Nombre { get; set; } = string.Empty;
}