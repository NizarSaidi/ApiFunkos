using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace FuncoApi.Entity;

/// <summary>Entidad relacional que representa una categoría de Funkos.</summary>
[Table("categorias")]
[Index(nameof(Nombre),IsUnique = true)]
public class CategoriaEntity
{
    /// <summary>Clave primaria textual de la categoría.</summary>
    [Key]
    public string Id { get; set; }
    
    /// <summary>Nombre obligatorio y único de la categoría.</summary>
    [Required]
    public string Nombre { get; set; }
}