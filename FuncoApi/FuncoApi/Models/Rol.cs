namespace FuncoApi.Models;

/// <summary>Representa un rol previsto para las cuentas de usuario.</summary>
public class Rol
{
    /// <summary>Identificador del rol.</summary>
    public int Id { get; set; }

    /// <summary>Nombre descriptivo del rol.</summary>
    public string Nombre { get; set; } = string.Empty;
}