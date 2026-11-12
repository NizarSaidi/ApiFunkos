namespace FuncoApi.Dto.FunkoDto;

/// <summary>Datos de un Funko devueltos por la API.</summary>
public class FunkoResponseDto
{
    /// <summary>Identificador del Funko.</summary>
    public int Id { get; set; }

    /// <summary>Nombre del Funko.</summary>
    public string Nombre { get; set; } = string.Empty;

    /// <summary>URL o referencia de la imagen.</summary>
    public string Imagen { get; set; } = string.Empty;

    /// <summary>Precio de venta.</summary>
    public decimal Precio { get; set; }

    /// <summary>Unidades disponibles.</summary>
    public int Stock { get; set; }

    /// <summary>Identificador de la categoría asociada.</summary>
    public string Categoria { get; set; } = string.Empty;

    /// <summary>Identificador del proveedor asociado.</summary>
    public int Proveedor { get; set; }
}