using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace FuncoApi.Entity;

/// <summary>Entidad de proveedor serializada en la colección MongoDB <c>proveedores</c>.</summary>
[Table("proveedores")]
[Index(nameof(Nombre), IsUnique = true)]
public class ProveedorEntity
{
    /// <summary>Identificador del documento y clave primaria.</summary>
    [BsonId]
    public int Id { get; set; }
    
    /// <summary>Nombre del proveedor, almacenado como <c>name</c> en MongoDB.</summary>
    [BsonElement("name")]
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