namespace FuncoApi.Dto.CategoriaDto;

/// <summary>Datos de una categoría devueltos por la API.</summary>
public class CategoriaResponseDto
{
    /// <summary>Identificador textual de la categoría.</summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>Nombre de la categoría.</summary>
    public string Nombre { get; set; } = string.Empty;
}