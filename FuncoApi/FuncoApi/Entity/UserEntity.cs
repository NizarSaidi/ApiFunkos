using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FuncoApi.Entity;

/// <summary>Registro de usuario persistido en la base de datos relacional.</summary>
public class UserEntity
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    /// <summary>Identificador del usuario.</summary>
    public Guid Id { get; set; }

    [Column("name")]
    /// <summary>Nombre del usuario.</summary>
    public string Nombre { get; set; } = string.Empty;

    [Column("email")]
    /// <summary>Dirección de correo electrónico del usuario.</summary>
    public string Email { get; set; } = string.Empty;

    [Column("password")]
    /// <summary>Campo de contraseña del modelo; el modelo no define su almacenamiento seguro.</summary>
    public string Password { get; set; } = string.Empty;

    [Column("role")] 
    /// <summary>rol asignado al usuario.</summary>
    public string Rol { get; set; } = string.Empty;
    
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