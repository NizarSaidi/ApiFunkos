using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace FuncoApi.Entity;

[Table("categorias")]
[Index(nameof(Nombre),IsUnique = true)]
public class CategoriaEntity
{
    [Key]
    public string Id { get; set; }
    
    [Required]
    public string Nombre { get; set; }
}