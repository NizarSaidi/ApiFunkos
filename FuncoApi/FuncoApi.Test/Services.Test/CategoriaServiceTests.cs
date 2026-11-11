using FuncoApi.Cache.objet;
using FuncoApi.Domain;
using FuncoApi.Entity;
using FuncoApi.Models;
using FuncoApi.Repositories;
using FuncoApi.Services.CategoriaService;
using Moq;

namespace FuncoApi.Test.Services.Test;

/// <summary>Comprueba validación, mapeo y errores del servicio de categorías.</summary>
public class CategoriaServiceTests
{
    /// <summary>Comprueba que una categoría no válida no se envía al repositorio.</summary>
    [Fact]
    public async Task Create_ReturnsValidationError_WithoutCallingRepository()
    {
        var (service, repository, _) = CreateService();

        var result = await service.Create(new Categoria { Id = "anime" });

        Assert.True(result.IsFailure);
        Assert.Equal(DomainErrorCode.Validation, result.Error.Code);
        repository.Verify(repo => repo.CreateAsync(It.IsAny<CategoriaEntity>()), Times.Never);
    }

    /// <summary>Comprueba el mapeo de entidad y la invalidación de caché al crear una categoría.</summary>
    [Fact]
    public async Task Create_MapsEntityAndInvalidatesListCache()
    {
        var (service, repository, cache) = CreateService();
        repository
            .Setup(repo => repo.CreateAsync(It.IsAny<CategoriaEntity>()))
            .ReturnsAsync((CategoriaEntity entity) => entity);

        var result = await service.Create(new Categoria { Id = "anime", Nombre = "Anime" });

        Assert.True(result.IsSuccess);
        Assert.Equal("anime", result.Value.Id);
        Assert.Equal("Anime", result.Value.Nombre);
        repository.Verify(repo => repo.CreateAsync(It.Is<CategoriaEntity>(entity =>
            entity.Id == "anime" && entity.Nombre == "Anime")), Times.Once);
        cache.Verify(service => service.RemoveAsync("categorias:all"), Times.Once);
        cache.Verify(service => service.RemoveAsync("categorias:anime"), Times.Once);
    }

    /// <summary>Comprueba que no se crea una categoría cuyo nombre ya está en uso.</summary>
    [Fact]
    public async Task Create_ReturnsConflict_WhenNameAlreadyExists()
    {
        var (service, repository, _) = CreateService();
        repository.Setup(repo => repo.ExistsByNameAsync("Anime", null)).ReturnsAsync(true);

        var result = await service.Create(new Categoria { Id = "anime", Nombre = "Anime" });

        Assert.Equal(DomainErrorCode.Conflict, result.Error.Code);
        Assert.Contains("Anime", result.Error.Message);
        repository.Verify(repo => repo.CreateAsync(It.IsAny<CategoriaEntity>()), Times.Never);
    }

    /// <summary>Comprueba el error de dominio cuando no existe la categoría solicitada.</summary>
    [Fact]
    public async Task GetById_ReturnsNotFound_WhenCategoryDoesNotExist()
    {
        var (service, repository, cache) = CreateService();
        repository.Setup(repo => repo.GetByIdAsync("missing")).ReturnsAsync((CategoriaEntity?)null);
        cache
            .Setup(service => service.GetAsync<Categoria>("categorias:missing"))
            .ReturnsAsync((Categoria?)null);

        var result = await service.GetById("missing");

        Assert.True(result.IsFailure);
        Assert.Equal(DomainErrorCode.NotFound, result.Error.Code);
    }

    /// <summary>Comprueba que una categoría cacheada no requiere una consulta al repositorio.</summary>
    [Fact]
    public async Task GetById_ReturnsCachedCategory_WhenAvailable()
    {
        var (service, repository, cache) = CreateService();
        var category = new Categoria { Id = "anime", Nombre = "Anime" };
        cache.Setup(item => item.GetAsync<Categoria>("categorias:anime")).ReturnsAsync(category);

        var result = await service.GetById("anime");

        Assert.True(result.IsSuccess);
        Assert.Same(category, result.Value);
        repository.Verify(repo => repo.GetByIdAsync(It.IsAny<string>()), Times.Never);
    }

    /// <summary>Comprueba que los identificadores vacíos producen errores de validación sin acceder al repositorio.</summary>
    [Theory]
    [InlineData("")]
    [InlineData("  ")]
    public async Task GetById_ReturnsValidationError_WhenIdIsMissing(string id)
    {
        var (service, repository, _) = CreateService();

        var result = await service.GetById(id);

        Assert.Equal(DomainErrorCode.Validation, result.Error.Code);
        repository.Verify(repo => repo.GetByIdAsync(It.IsAny<string>()), Times.Never);
    }

    /// <summary>Comprueba el error de persistencia cuando no se crea la categoría.</summary>
    [Fact]
    public async Task Create_ReturnsPersistenceFailure_WhenRepositoryReturnsNull()
    {
        var (service, repository, _) = CreateService();
        repository.Setup(repo => repo.CreateAsync(It.IsAny<CategoriaEntity>()))
            .ReturnsAsync((CategoriaEntity?)null);

        var result = await service.Create(new Categoria { Id = "anime", Nombre = "Anime" });

        Assert.Equal(DomainErrorCode.PersistenceFailure, result.Error.Code);
    }

    /// <summary>Comprueba el error no encontrado cuando el repositorio no actualiza la categoría.</summary>
    [Fact]
    public async Task Update_ReturnsNotFound_WhenRepositoryReturnsNull()
    {
        var (service, repository, _) = CreateService();
        repository.Setup(repo => repo.UpdateAsync(It.IsAny<CategoriaEntity>()))
            .ReturnsAsync((CategoriaEntity?)null);

        var result = await service.Update("anime", new Categoria { Id = "anime", Nombre = "Anime" });

        Assert.Equal(DomainErrorCode.NotFound, result.Error.Code);
    }

    /// <summary>Comprueba que actualizar con el nombre de otra categoría produce un conflicto.</summary>
    [Fact]
    public async Task Update_ReturnsConflict_WhenNameBelongsToAnotherCategory()
    {
        var (service, repository, _) = CreateService();
        repository.Setup(repo => repo.ExistsByNameAsync("Anime", "manga")).ReturnsAsync(true);

        var result = await service.Update("manga", new Categoria { Id = "manga", Nombre = "Anime" });

        Assert.Equal(DomainErrorCode.Conflict, result.Error.Code);
        repository.Verify(repo => repo.UpdateAsync(It.IsAny<CategoriaEntity>()), Times.Never);
    }

    /// <summary>Comprueba el error no encontrado cuando el repositorio no elimina la categoría.</summary>
    [Fact]
    public async Task Delete_ReturnsNotFound_WhenRepositoryReturnsNull()
    {
        var (service, repository, _) = CreateService();
        repository.Setup(repo => repo.DeleteAsync("anime")).ReturnsAsync((CategoriaEntity?)null);

        var result = await service.Delete("anime");

        Assert.Equal(DomainErrorCode.NotFound, result.Error.Code);
    }

    /// <summary>Comprueba que un identificador distinto al del modelo falla antes de persistir.</summary>
    [Fact]
    public async Task Update_ReturnsValidationError_WhenIdsDiffer()
    {
        var (service, repository, _) = CreateService();

        var result = await service.Update("anime", new Categoria { Id = "manga", Nombre = "Manga" });

        Assert.True(result.IsFailure);
        Assert.Equal(DomainErrorCode.Validation, result.Error.Code);
        repository.Verify(repo => repo.UpdateAsync(It.IsAny<CategoriaEntity>()), Times.Never);
    }

    /// <summary>Crea el servicio con dependencias simuladas para aislar cada caso de prueba.</summary>
    private static (CategoriaService Service, Mock<ICategoriaRepository> Repository, Mock<ICacheService> Cache)
        CreateService()
    {
        var repository = new Mock<ICategoriaRepository>();
        var cache = new Mock<ICacheService>();
        cache
            .Setup(service => service.RemoveAsync(It.IsAny<string>()))
            .Returns(Task.CompletedTask);

        return (new CategoriaService(cache.Object, repository.Object), repository, cache);
    }
}
