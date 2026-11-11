using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace FuncoApi.Entity;

[Table("funkos")]
[Index(nameof(Nombre), IsUnique = true)]
public class FunkoEntity
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }
    
    [Required]
    [Column("nombre")]
    public string Nombre { get; set; } = string.Empty;
    
    [Column("imagen")]
    public string Imagen { get; set; } = string.Empty;
    
    [Column("precio")]
    [Range(0.1 , double.MaxValue, ErrorMessage = "El precio debe ser mayor o igual a cero.")]
    public decimal Precio { get; set; }
    
    [Column("stock")]
    [Range(0, int.MaxValue, ErrorMessage = "El stock debe ser un número entero positivo.")]
    public int Stock { get; set; }
    
    [Column("categorias_id")]
    [ForeignKey("categorias")]
    public string Categoria { get; set; }
    
    [Column("proveedor_id")]
    [ForeignKey("proveedores")]
    public int Proveedor { get; set; }
    
    [Column("createAt")]
    public DateTime CreatedAt { get; set; }
    
    [Column("updateAt")]
    public DateTime UpdateAt { get; set; }
    
    [Column("deleteAt")]
    public DateTime? DeleteAt { get; set; }
    
    [Column("isDeleted")]
    public bool IsDeleted { get; set; }
}