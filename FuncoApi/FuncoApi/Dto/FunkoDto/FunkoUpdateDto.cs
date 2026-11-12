using System.ComponentModel.DataAnnotations;

namespace FuncoApi.Dto.FunkoDto;

/// <summary>Datos editables de un Funko; su identificador se recibe en la ruta.</summary>
public class FunkoUpdateDto
{
    /// <summary>Nombre único del Funko.</summary>
    [Required]
    public string Nombre { get; set; } = string.Empty;

    /// <summary>URL o referencia de la imagen.</summary>
    public string Imagen { get; set; } = string.Empty;

    /// <summary>Precio del Funko; debe ser como mínimo 0,1.</summary>
    [Range(0.1, double.MaxValue)]
    public decimal Precio { get; set; }

    /// <summary>Unidades disponibles; no puede ser negativo.</summary>
    [Range(0, int.MaxValue)]
    public int Stock { get; set; }

    /// <summary>Identificador de la categoría asociada.</summary>
    [Required]
    public string Categoria { get; set; } = string.Empty;

    /// <summary>Identificador positivo del proveedor asociado.</summary>
    [Range(1, int.MaxValue)]
    public int Proveedor { get; set; }
}