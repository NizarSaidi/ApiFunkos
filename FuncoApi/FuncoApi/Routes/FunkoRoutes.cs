using CSharpFunctionalExtensions;
using FuncoApi.Domain;
using FuncoApi.Models;
using FuncoApi.Services.FunkosService;
using IResult = Microsoft.AspNetCore.Http.IResult;

namespace FunkoApi.Routes;

/// <summary>Registra los endpoints HTTP de gestión de Funkos.</summary>
public static class FunkoRoutes
{
    /// <summary>Mapea las rutas CRUD de Funkos bajo <c>/api/funcopops</c>.</summary>
    /// <param name="app">Aplicación ASP.NET Core en la que se registran las rutas.</param>
    public static void MapFunkosRoutes(this WebApplication app)
    {
        var group = app.MapGroup("/api/funcopops").WithTags("Funcopops");

        // Las operaciones devuelven el resultado del servicio convertido a HTTP.
        group.MapGet("/", async (IFunkosService service) =>
            ToHttpResult(await service.GetAll()));

        group.MapGet("/{id:int}", async (int id, IFunkosService service) =>
            ToHttpResult(await service.GetById(id)));

        group.MapPost("/", async (Funko funko, IFunkosService service) =>
        {
            var result = await service.Create(funko);
            return result.IsSuccess
                ? Results.Created($"/api/funcopops/{result.Value.Id}", result.Value)
                : ToHttpResult(result);
        });

        group.MapDelete("/{id:int}", async (int id, IFunkosService service) =>
            ToHttpResult(await service.Delete(id)));

        group.MapPut("/{id:int}", async (int id, Funko funko, IFunkosService service) =>
            ToHttpResult(await service.Update(id, funko)));
    }

    /// <summary>Convierte el resultado de dominio a una respuesta HTTP.</summary>
    private static IResult ToHttpResult<T>(Result<T, DomainError> result)
    {
        if (result.IsSuccess)
        {
            return Results.Ok(result.Value);
        }

        return result.Error.Code switch
        {
            DomainErrorCode.Validation => Results.BadRequest(result.Error),
            DomainErrorCode.NotFound => Results.NotFound(result.Error),
            DomainErrorCode.Conflict => Results.Conflict(result.Error),
            _ => Results.Problem(result.Error.Message)
        };
    }
}