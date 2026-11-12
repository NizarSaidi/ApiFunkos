using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace FuncoApi.Entity;

/// <summary>Entidad de persistencia relacional que representa un Funko.</summary>
[Table("funkos")]
[Index(nameof(Nombre), IsUnique = true)]
public class FunkoEntity
{
    /// <summary>Clave primaria generada por la base de datos.</summary>
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }
    
    /// <summary>Nombre único y obligatorio del Funko.</summary>
    [Required]
    [Column("nombre")]
    public string Nombre { get; set; } = string.Empty;
    
    /// <summary>URL o referencia de la imagen asociada.</summary>
    [Column("imagen")]
    public string Imagen { get; set; } = string.Empty;
    
    /// <summary>Precio del Funko; debe ser al menos 0,1.</summary>
    [Column("precio")]
    [Range(0.1 , double.MaxValue, ErrorMessage = "El precio debe ser mayor o igual a cero.")]
    public decimal Precio { get; set; }
    
    /// <summary>Unidades disponibles; debe ser un entero no negativo.</summary>
    [Column("stock")]
    [Range(0, int.MaxValue, ErrorMessage = "El stock debe ser un número entero positivo.")]
    public int Stock { get; set; }
    
    /// <summary>Identificador de la categoría asociada.</summary>
    [Column("categorias_id")]
    [ForeignKey("categorias")]
    public string Categoria { get; set; }
    
    /// <summary>Identificador del proveedor asociado.</summary>
    [Column("proveedor_id")]
    [ForeignKey("proveedores")]
    public int Proveedor { get; set; }
    
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