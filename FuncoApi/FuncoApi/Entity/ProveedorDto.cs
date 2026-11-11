using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace FuncoApi.Entity;

[Table("proveedores")]
[Index(nameof(Nombre), IsUnique = true)]
public class ProveedorEntity
{
    [BsonId]
    public int Id { get; set; }
    
    [BsonElement("name")]
    public string Nombre { get; set; }
}