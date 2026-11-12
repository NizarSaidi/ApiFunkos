using FuncoApi.Context;
using FuncoApi.Entity;
using FuncoApi.Repositories;
using Microsoft.EntityFrameworkCore;

namespace FuncoApi.Test.Repositories.IntegrationTest;

/// <summary>Prueba los repositorios relacionales contra EF Core InMemory real.</summary>
public class RelationalRepositoryIntegrationTests
{
    [Fact]
    public async Task FunkoRepository_PaginatesUpdatesAndSoftDeletes()
    {
        await using var context = CreateContext();
        var repository = new FunkoRepository(context);
        var first = await repository.CreateAsync(CreateFunko("Spider-Man"));
        var second = await repository.CreateAsync(CreateFunko("Iron Man"));
        await repository.CreateAsync(CreateFunko("Thor"));

        Assert.NotNull(first);
        Assert.True(first.Id > 0);
        Assert.False(first.IsDeleted);
        Assert.NotEqual(default, first.CreatedAt);
        Assert.True(await repository.ExistsByNameAsync("Spider-Man"));
        Assert.False(await repository.ExistsByNameAsync("Spider-Man", first.Id));

        var page = await repository.GetAllAsync(page: 2, pageSize: 1);
        Assert.True(page.IsSuccess);
        Assert.Equal(3, page.Value.Count);
        Assert.Equal("Iron Man", Assert.Single(page.Value.Objs).Nombre);

        first.Nombre = "Spider-Man actualizado";
        var updated = await repository.UpdateAsync(first);
        Assert.NotNull(updated);
        Assert.Equal("Spider-Man actualizado", (await repository.GetByIdAsync(first.Id))!.Nombre);
        Assert.False(await repository.ExistsByNameAsync("Spider-Man"));

        var deleted = await repository.DeleteAsync(second!.Id);
        Assert.NotNull(deleted);
        Assert.True(deleted.IsDeleted);
        Assert.NotNull(deleted.DeleteAt);
        Assert.Null(await repository.GetByIdAsync(second.Id));
        var afterDelete = await repository.GetAllAsync(page: 1, pageSize: 10);
        Assert.Equal(2, afterDelete.Value.Count);
        Assert.DoesNotContain(afterDelete.Value.Objs, funko => funko.Id == second.Id);
    }

    [Fact]
    public async Task CategoriaRepository_CreatesGeneratedIdAndCompletesCrud()
    {
        await using var context = CreateContext();
        var repository = new CategoriaRepository(context);
        var created = await repository.CreateAsync(new CategoriaEntity
        {
            Id = "ignored",
            Nombre = "Anime"
        });

        Assert.NotNull(created);
        Assert.False(string.IsNullOrWhiteSpace(created.Id));
        Assert.True(await repository.ExistsByNameAsync("Anime"));
        Assert.False(await repository.ExistsByNameAsync("Anime", created.Id));
        Assert.Equal(created.Id, (await repository.GetByIdAsync(created.Id))!.Id);

        var second = await repository.CreateAsync(new CategoriaEntity
        {
            Id = "ignored-again",
            Nombre = "Manga"
        });
        var page = await repository.GetAllAsync(page: 2, pageSize: 1);
        Assert.True(page.IsSuccess);
        Assert.Equal(2, page.Value.Count);
        Assert.Equal("Manga", Assert.Single(page.Value.Objs).Nombre);

        created.Nombre = "Animación";
        var updated = await repository.UpdateAsync(created);
        Assert.NotNull(updated);
        Assert.Equal("Animación", (await repository.GetByIdAsync(created.Id))!.Nombre);

        var deleted = await repository.DeleteAsync(second!.Id);
        Assert.NotNull(deleted);
        Assert.Null(await repository.GetByIdAsync(second.Id));
        Assert.Equal(1, (await repository.GetAllAsync(1, 10)).Value.Count);
    }

    private static FuncoDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<FuncoDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new FuncoDbContext(options);
    }

    private static FunkoEntity CreateFunko(string name) => new()
    {
        Nombre = name,
        Imagen = $"{name}.png",
        Precio = 15m,
        Stock = 4,
        Categoria = "anime",
        Proveedor = 1
    };
}
