namespace FuncoApi.Configure;

public static class ConfigurationApp
{
    public static string Mode => Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development";
}