namespace FuncoApi.Dto.ProveedorDto;

/// <summary>Datos de un proveedor devueltos por la API.</summary>
public class ProveedorResponseDto
{
    /// <summary>Identificador del proveedor.</summary>
    public int Id { get; set; }

    /// <summary>Nombre del proveedor.</summary>
    public string Nombre { get; set; } = string.Empty;
}