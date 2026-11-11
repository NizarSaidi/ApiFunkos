namespace FuncoApi.Models;

/// <summary>Representa un Funko recibido o devuelto por la API.</summary>
public class Funko
{
    /// <summary>Identificador asignado por la persistencia.</summary>
    public int Id { get; set; }

    /// <summary>Nombre visible del Funko.</summary>
    public string Nombre { get; set; } = string.Empty;

    /// <summary>URL o referencia de la imagen del Funko.</summary>
    public string Imagen { get; set; } = string.Empty;

    /// <summary>Precio de venta del Funko.</summary>
    public decimal Precio { get; set; }

    /// <summary>Unidades disponibles en inventario.</summary>
    public int Stock { get; set; }

    /// <summary>Identificador de la categoría asociada.</summary>
    public string Categoria { get; set; } = string.Empty;

    /// <summary>Identificador del proveedor asociado.</summary>
    public int Proveedor { get; set; }
}