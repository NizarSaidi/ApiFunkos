namespace FunkoApi.Routes;

/// <summary>Opciones de configuración utilizadas para crear tokens JWT.</summary>
public class JwtSettings
{
        /// <summary>Clave simétrica usada para firmar los tokens.</summary>
        public string Key { get; set; } = string.Empty;

        /// <summary>Emisor esperado del token.</summary>
        public string Issuer { get; set; } = string.Empty;

        /// <summary>Audiencia para la que se emite el token.</summary>
        public string Audience { get; set; } = string.Empty;

        /// <summary>Duración del token en minutos.</summary>
        public int DurationInMinutes { get; set; }
}