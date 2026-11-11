using FuncoApi.Entity;
using FuncoApi.Repositories;

namespace FunkoApi.Routes;

public static class FunkoRoutes
{
    public static void MapFunkosRoutes(this WebApplication app)
    {
        var group = app.MapGroup("/api/funcopops").WithTags("Funcopops");

        group.MapGet("/", async (IFunkoRepository repository) =>
            Results.Ok(await repository.GetAllAsync()));
        group.MapGet("/{id:int}", async (int id, IFunkoRepository repository) =>
        {
            var funko = await repository.GetByIdAsync(id);
            return funko is not null ? Results.Ok(funko) : Results.NotFound();
        });
        
        group.MapPost("/", async (FunkoEntity funko, IFunkoRepository repository) =>
        {
            var createdId = await repository.CreateAsync(funko);
            return Results.Created($"/api/funcopops/{createdId}", createdId);
        });
        
        group.MapDelete("/{id:int}", async (int id, IFunkoRepository repository) =>
        {
            var deletedFunko = await repository.DeleteAsync(id);
            return deletedFunko is not null ? Results.Ok(deletedFunko) : Results.NotFound();
        });
        
        group.MapPut("/{id:int}", async (int id, FunkoEntity funko, IFunkoRepository repository) =>
        {
            if (id != funko.Id)
            {
                return Results.BadRequest();
            }

            var updatedFunko = await repository.UpdateAsync(funko);
            return updatedFunko is not null ? Results.Ok(updatedFunko) : Results.NotFound();
        });
    }
}