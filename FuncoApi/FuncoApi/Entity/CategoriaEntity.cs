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
    
    /// <summary>Fecha UTC de creación del registro.</summary>
    [Column("createAt")]
    public DateTime CreatedAt { get; set; }
    
    /// <summary>Fecha UTC de la última modificación.</summary>
    [Column("updateAt")]
    public DateTime UpdateAt { get; set; }
    
    /// <summary>Fecha UTC en que se marcó como eliminado; nula si sigue activo.</summary>
    [Column("deleteAt")]
    public DateTime? DeleteAt { get; set; }
    
    /// <summary>Indica si el registro se ha eliminado lógicamente.</summary>
    [Column("isDeleted")]
    public bool IsDeleted { get; set; }
}