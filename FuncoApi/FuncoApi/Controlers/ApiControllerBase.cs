using FuncoApi.Domain;
using Microsoft.AspNetCore.Mvc;

namespace FuncoApi.Controlers;

/// <summary>Proporciona la conversión común de errores de dominio a respuestas HTTP.</summary>
public abstract class ApiControllerBase : ControllerBase
{
    /// <summary>Convierte un error de dominio al código HTTP correspondiente.</summary>
    /// <param name="error">Error que devolvió el servicio.</param>
    /// <returns>Respuesta HTTP con los detalles del error.</returns>
    protected ObjectResult ToHttpError(DomainError error)
    {
        var statusCode = error.Code switch
        {
            DomainErrorCode.Validation => StatusCodes.Status400BadRequest,
            DomainErrorCode.NotFound => StatusCodes.Status404NotFound,
            DomainErrorCode.Conflict => StatusCodes.Status409Conflict,
            _ => StatusCodes.Status500InternalServerError
        };

        return new ObjectResult(error) { StatusCode = statusCode };
    }
}
