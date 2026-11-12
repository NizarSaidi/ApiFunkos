namespace FuncoApi.Configure;

/// <summary>Proporciona la configuración del entorno de ejecución.</summary>
public static class ConfigurationApp
{
    /// <summary>Entorno ASP.NET Core activo, con <c>Development</c> como valor predeterminado.</summary>
    public static string Mode => Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development";
    
    /// <summary> Retorna el tamaño de página.</summary>
    public static int PageSize => int.Parse(Environment.GetEnvironmentVariable("PAGE_SIZE") ?? "10");
}