using System.ComponentModel.DataAnnotations;

namespace FuncoApi.Dto.ProveedorDto;

/// <summary>Datos necesarios para crear un proveedor.</summary>
public class ProveedorCreateDto
{
    /// <summary>Nombre único del proveedor.</summary>
    [Required]
    public string Nombre { get; set; } = string.Empty;
}