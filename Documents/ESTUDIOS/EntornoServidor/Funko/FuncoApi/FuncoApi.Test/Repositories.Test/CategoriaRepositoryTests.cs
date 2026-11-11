using FuncoApi.Entity;
using FuncoApi.Repositories;
using Moq;

namespace FuncoApi.Test.Repositories.Test;

public class CategoriaRepositoryTests
{
    [Fact]
    public async Task CreateAsync_AddsCategoriaToDbSet()
    {
        var data = new List<CategoriaEntity>();
        var dbSet = RepositoryTestContext.CreateDbSet(data);
        var context = RepositoryTestContext.CreateContext();
        context.Setup(db => db.Categorias).Returns(dbSet.Object);
        var repository = new CategoriaRepository(context.Object);
        var categoria = new CategoriaEntity { Id = "anime", Nombre = "Anime" };

        var created = await repository.CreateAsync(categoria);

        Assert.NotNull(created);
        Assert.Equal("anime", created.Id);
        Assert.Same(categoria, data.Single());
        context.Verify(db => db.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetAllAsync_ReturnsAllCategorias()
    {
        var data = new List<CategoriaEntity>
        {
            new() { Id = "anime", Nombre = "Anime" },
            new() { Id = "serie", Nombre = "Serie" }
        };
        var dbSet = RepositoryTestContext.CreateDbSet(data);
        var context = RepositoryTestContext.CreateContext();
        context.Setup(db => db.Categorias).Returns(dbSet.Object);
        var repository = new CategoriaRepository(context.Object);

        var result = await repository.GetAllAsync();

        Assert.Equal(2, result.Count());
        Assert.Contains(result, item => item.Id == "anime");
        Assert.Contains(result, item => item.Id == "serie");
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsCategoria_WhenExists()
    {
        var data = new List<CategoriaEntity>
        {
            new() { Id = "anime", Nombre = "Anime" }
        };
        var dbSet = RepositoryTestContext.CreateDbSet(data);
        var context = RepositoryTestContext.CreateContext();
        context.Setup(db => db.Categorias).Returns(dbSet.Object);
        var repository = new CategoriaRepository(context.Object);

        var result = await repository.GetByIdAsync("anime");

        Assert.NotNull(result);
        Assert.Equal("anime", result.Id);
        Assert.Equal("Anime", result.Nombre);
    }

    [Fact]
    public async Task UpdateAsync_UpdatesExistingCategoria()
    {
        var data = new List<CategoriaEntity>
        {
            new() { Id = "anime", Nombre = "Anime" }
        };
        var dbSet = RepositoryTestContext.CreateDbSet(data);
        var context = RepositoryTestContext.CreateContext();
        context.Setup(db => db.Categorias).Returns(dbSet.Object);
        var repository = new CategoriaRepository(context.Object);

        var updated = await repository.UpdateAsync(new CategoriaEntity { Id = "anime", Nombre = "Animación" });

        Assert.NotNull(updated);
        Assert.Equal("Animación", updated.Nombre);
        Assert.Equal("Animación", data.Single().Nombre);
        context.Verify(db => db.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_ReturnsNull_WhenCategoriaDoesNotExist()
    {
        var data = new List<CategoriaEntity>();
        var dbSet = RepositoryTestContext.CreateDbSet(data);
        var context = RepositoryTestContext.CreateContext();
        context.Setup(db => db.Categorias).Returns(dbSet.Object);
        var repository = new CategoriaRepository(context.Object);

        var updated = await repository.UpdateAsync(new CategoriaEntity { Id = "missing", Nombre = "No existe" });

        Assert.Null(updated);
    }

    [Fact]
    public async Task DeleteAsync_RemovesCategoria_WhenExists()
    {
        var data = new List<CategoriaEntity>
        {
            new() { Id = "anime", Nombre = "Anime" }
        };
        var dbSet = RepositoryTestContext.CreateDbSet(data);
        var context = RepositoryTestContext.CreateContext();
        context.Setup(db => db.Categorias).Returns(dbSet.Object);
        var repository = new CategoriaRepository(context.Object);

        var deleted = await repository.DeleteAsync("anime");

        Assert.NotNull(deleted);
        Assert.Equal("anime", deleted.Id);
        Assert.Empty(data);
        dbSet.Verify(set => set.Remove(It.IsAny<CategoriaEntity>()), Times.Once);
        context.Verify(db => db.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_ReturnsNull_WhenCategoriaDoesNotExist()
    {
        var data = new List<CategoriaEntity>();
        var dbSet = RepositoryTestContext.CreateDbSet(data);
        var context = RepositoryTestContext.CreateContext();
        context.Setup(db => db.Categorias).Returns(dbSet.Object);
        var repository = new CategoriaRepository(context.Object);

        var deleted = await repository.DeleteAsync("missing");

        Assert.Null(deleted);
    }
}
