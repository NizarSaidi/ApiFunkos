using System.ComponentModel.DataAnnotations;

namespace FuncoApi.Dto.CategoriaDto;

/// <summary>Datos editables de una categoría; su identificador se recibe en la ruta.</summary>
public class CategoriaUpdateDto
{
    /// <summary>Nombre único de la categoría.</summary>
    [Required]
    public string Nombre { get; set; } = string.Empty;
}