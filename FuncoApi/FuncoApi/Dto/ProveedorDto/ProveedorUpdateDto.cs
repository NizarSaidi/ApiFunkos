using System.ComponentModel.DataAnnotations;

namespace FuncoApi.Dto.ProveedorDto;

/// <summary>Datos editables de un proveedor; su identificador se recibe en la ruta.</summary>
public class ProveedorUpdateDto
{
    /// <summary>Nombre único del proveedor.</summary>
    [Required]
    public string Nombre { get; set; } = string.Empty;
}