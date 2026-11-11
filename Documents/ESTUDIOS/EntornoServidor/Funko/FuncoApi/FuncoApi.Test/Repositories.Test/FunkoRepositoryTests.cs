using FuncoApi.Entity;
using FuncoApi.Repositories;
using Moq;

namespace FuncoApi.Test.Repositories.Test;

public class FunkoRepositoryTests
{
    [Fact]
    public async Task CreateAsync_GeneratesIdAndSavesFunko()
    {
        var data = new List<FunkoEntity>();
        var dbSet = RepositoryTestContext.CreateDbSet(data, funko => funko.Id = data.Count);
        var context = RepositoryTestContext.CreateContext();
        context.Setup(db => db.Funkos).Returns(dbSet.Object);
        var repository = new FunkoRepository(context.Object);
        var funko = CreateFunko();
        funko.Id = 42;

        var created = await repository.CreateAsync(funko);

        Assert.True(created.Id > 0);
        Assert.NotEqual(42, created.Id);
        Assert.False(created.IsDeleted);
        Assert.Null(created.DeleteAt);
        Assert.NotEqual(default, created.CreatedAt);
        Assert.Equal(created.CreatedAt, created.UpdateAt);
        Assert.Same(funko, data.Single());
        context.Verify(db => db.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreateAsync_GeneratesNextId_WhenSeveralItemsAlreadyExist()
    {
        var existing = new List<FunkoEntity>
        {
            CreateFunkoWithId(1),
            CreateFunkoWithId(2),
            CreateFunkoWithId(3)
        };
        var dbSet = RepositoryTestContext.CreateDbSet(existing, funko => funko.Id = existing.Count);
        var context = RepositoryTestContext.CreateContext();
        context.Setup(db => db.Funkos).Returns(dbSet.Object);
        var repository = new FunkoRepository(context.Object);
        var funko = CreateFunko();

        var created = await repository.CreateAsync(funko);

        Assert.Equal(existing.Count, created.Id);
        Assert.Equal(4, created.Id);
        Assert.Equal(4, existing.Count);
        Assert.Same(funko, existing[^1]);
        context.Verify(db => db.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetAllAsync_ExcludesSoftDeletedFunkos()
    {
        var data = new List<FunkoEntity>
        {
            CreateFunko("Visible"),
            new() { Id = 2, Nombre = "Deleted", IsDeleted = true }
        };
        var dbSet = RepositoryTestContext.CreateDbSet(data);
        var context = RepositoryTestContext.CreateContext();
        context.Setup(db => db.Funkos).Returns(dbSet.Object);
        var repository = new FunkoRepository(context.Object);

        var result = await repository.GetAllAsync();

        Assert.Single(result);
        Assert.Equal("Visible", result[0].Nombre);
    }

    [Fact]
    public async Task DeleteAsync_SoftDeletesFunkoAndReturnsNullWhenMissingOrAlreadyDeleted()
    {
        var funko = CreateFunko();
        funko.Id = 1;
        var data = new List<FunkoEntity> { funko };
        var dbSet = RepositoryTestContext.CreateDbSet(data);
        var context = RepositoryTestContext.CreateContext();
        context.Setup(db => db.Funkos).Returns(dbSet.Object);
        var repository = new FunkoRepository(context.Object);

        var deleted = await repository.DeleteAsync(funko.Id);

        Assert.Same(funko, deleted);
        Assert.True(funko.IsDeleted);
        Assert.NotNull(funko.DeleteAt);
        Assert.Equal(funko.DeleteAt, (DateTime?)funko.UpdateAt);
        Assert.Null(await repository.GetByIdAsync(funko.Id));
        Assert.Null(await repository.DeleteAsync(funko.Id));
        Assert.Single(data);
        dbSet.Verify(set => set.Remove(It.IsAny<FunkoEntity>()), Times.Never);
        context.Verify(db => db.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_UpdatesEditableFieldsWithoutChangingSoftDeleteState()
    {
        var original = CreateFunko();
        original.Id = 1;
        var data = new List<FunkoEntity> { original };
        var dbSet = RepositoryTestContext.CreateDbSet(data);
        var context = RepositoryTestContext.CreateContext();
        context.Setup(db => db.Funkos).Returns(dbSet.Object);
        var repository = new FunkoRepository(context.Object);
        var update = CreateFunko("Updated");
        update.Id = original.Id;
        update.Imagen = "updated.png";
        update.Precio = 19.99m;
        update.Stock = 8;
        update.Categoria = "Anime";
        update.Proveedor = 5;
        update.IsDeleted = true;
        update.DeleteAt = DateTime.UtcNow;
        var beforeUpdate = original.UpdateAt;

        var result = await repository.UpdateAsync(update);

        Assert.Same(original, result);
        Assert.Equal("Updated", original.Nombre);
        Assert.Equal("updated.png", original.Imagen);
        Assert.Equal(19.99m, original.Precio);
        Assert.Equal(8, original.Stock);
        Assert.Equal("Anime", original.Categoria);
        Assert.Equal(5, original.Proveedor);
        Assert.False(original.IsDeleted);
        Assert.Null(original.DeleteAt);
        Assert.NotEqual(default, original.UpdateAt);
        Assert.NotEqual(beforeUpdate, original.UpdateAt);
        Assert.Null(await repository.UpdateAsync(CreateFunkoWithId(int.MaxValue)));
        context.Verify(db => db.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsFunko_WhenExists()
    {
        var funko = CreateFunko("Visible");
        funko.Id = 7;
        var data = new List<FunkoEntity> { funko };
        var dbSet = RepositoryTestContext.CreateDbSet(data);
        var context = RepositoryTestContext.CreateContext();
        context.Setup(db => db.Funkos).Returns(dbSet.Object);
        var repository = new FunkoRepository(context.Object);

        var result = await repository.GetByIdAsync(funko.Id);

        Assert.NotNull(result);
        Assert.Same(funko, result);
        Assert.Equal("Visible", result.Nombre);
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsNullForUnknownId()
    {
        var dbSet = RepositoryTestContext.CreateDbSet(new List<FunkoEntity>());
        var context = RepositoryTestContext.CreateContext();
        context.Setup(db => db.Funkos).Returns(dbSet.Object);
        var repository = new FunkoRepository(context.Object);

        Assert.Null(await repository.GetByIdAsync(123));
    }

    private static FunkoEntity CreateFunko(string nombre = "Funko")
    {
        return new FunkoEntity
        {
            Nombre = nombre,
            Imagen = "funko.png",
            Precio = 12.50m,
            Stock = 3,
            Categoria = "Series",
            Proveedor = 1
        };
    }

    private static FunkoEntity CreateFunkoWithId(int id)
    {
        var funko = CreateFunko();
        funko.Id = id;
        return funko;
    }
}
