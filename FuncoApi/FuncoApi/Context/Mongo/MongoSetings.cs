namespace FuncoApi.Context.Mongo;

/// <summary>Opciones de conexión y nombre de base de datos para MongoDB.</summary>
public class MongoSettings
{
    /// <summary>Cadena de conexión del servidor MongoDB.</summary>
    public string ConnectionString { get; set; }

    /// <summary>Base de datos que contiene la colección de proveedores.</summary>
    public string DatabaseName { get; set; }
}