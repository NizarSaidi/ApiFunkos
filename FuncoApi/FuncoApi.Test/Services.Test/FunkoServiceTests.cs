using FuncoApi.Cache.objet;
using FuncoApi.Configure;
using FuncoApi.Domain;
using FuncoApi.Entity;
using FuncoApi.Models;
using FuncoApi.Repositories;
using FuncoApi.Services.FunkosService;
using CSharpFunctionalExtensions;
using Moq;

namespace FuncoApi.Test.Services.Test;

/// <summary>Comprueba validación, mapeo, errores e invalidación de caché del servicio de Funkos.</summary>
public class FunkoServiceTests
{
    /// <summary>Comprueba que el servicio pasa el número de página y el tamaño configurado al repositorio.</summary>
    [Fact]
    public async Task GetAll_RequestsPageFromRepository()
    {
        var (service, repository, _) = CreateService();
        repository.Setup(repo => repo.GetAllAsync(2, ConfigurationApp.PageSize))
            .ReturnsAsync(Result.Success<(IReadOnlyList<FunkoEntity> Objs, int Count), DomainError>(
                (new List<FunkoEntity> { new() { Id = 20, Nombre = "Página 2" } }, 21)));

        var result = await service.GetAll(2);

        Assert.True(result.IsSuccess);
        var funko = Assert.Single(result.Value.Objs);
        Assert.Equal(20, funko.Id);
        Assert.Equal("Página 2", funko.Nombre);
        Assert.Equal(21, result.Value.Count);
        repository.Verify(repo => repo.GetAllAsync(2, ConfigurationApp.PageSize), Times.Once);
    }

    /// <summary>Comprueba que el servicio propaga el error de persistencia del repositorio.</summary>
    [Fact]
    public async Task GetAll_PropagatesRepositoryDomainError()
    {
        var (service, repository, _) = CreateService();
        var error = Errors.PersistenceFailure("los Funkos");
        repository.Setup(repo => repo.GetAllAsync(1, ConfigurationApp.PageSize))
            .ReturnsAsync(Result.Failure<(IReadOnlyList<FunkoEntity> Objs, int Count), DomainError>(error));

        var result = await service.GetAll();

        Assert.True(result.IsFailure);
        Assert.Equal(error, result.Error);
    }

    /// <summary>Comprueba que una página inválida devuelve un error de dominio sin consultar persistencia.</summary>
    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task GetAll_ReturnsValidationError_WhenPageIsNotPositive(int page)
    {
        var (service, repository, _) = CreateService();

        var result = await service.GetAll(page);

        Assert.Equal(DomainErrorCode.Validation, result.Error.Code);
        repository.Verify(repo => repo.GetAllAsync(It.IsAny<int>(), It.IsAny<int>()), Times.Never);
    }

    /// <summary>Comprueba que la validación fallida evita llamar al repositorio.</summary>
    [Fact]
    public async Task Create_ReturnsValidationError_WithoutCallingRepository()
    {
        var (service, repository, _) = CreateService();

        var result = await service.Create(new Funko { Precio = 10, Stock = 1, Categoria = "Series", Proveedor = 1 });

        Assert.True(result.IsFailure);
        Assert.Equal(DomainErrorCode.Validation, result.Error.Code);
        repository.Verify(repo => repo.CreateAsync(It.IsAny<FunkoEntity>()), Times.Never);
    }

    /// <summary>Comprueba el mapeo de entidad y la invalidación de la lista al crear.</summary>
    [Fact]
    public async Task Create_MapsToEntityAndReturnsCreatedModel()
    {
        var (service, repository, cache) = CreateService();
        repository
            .Setup(repo => repo.CreateAsync(It.IsAny<FunkoEntity>()))
            .ReturnsAsync((FunkoEntity entity) =>
            {
                entity.Id = 12;
                return entity;
            });
        var funko = CreateValidFunko();

        var result = await service.Create(funko);

        Assert.True(result.IsSuccess);
        Assert.Equal(12, result.Value.Id);
        Assert.Equal(funko.Nombre, result.Value.Nombre);
        repository.Verify(repo => repo.CreateAsync(It.Is<FunkoEntity>(entity =>
            entity.Nombre == funko.Nombre &&
            entity.Categoria == funko.Categoria &&
            entity.Proveedor == funko.Proveedor)), Times.Once);
        cache.Verify(service => service.RemoveAsync("funkos:all"), Times.Once);
    }

    /// <summary>Comprueba que no se persiste un Funko si ya hay otro con el mismo nombre.</summary>
    [Fact]
    public async Task Create_ReturnsConflict_WhenNameAlreadyExists()
    {
        var (service, repository, _) = CreateService();
        repository.Setup(repo => repo.ExistsByNameAsync("Funko", null)).ReturnsAsync(true);

        var result = await service.Create(CreateValidFunko());

        Assert.Equal(DomainErrorCode.Conflict, result.Error.Code);
        Assert.Contains("Funko", result.Error.Message);
        repository.Verify(repo => repo.CreateAsync(It.IsAny<FunkoEntity>()), Times.Never);
    }

    /// <summary>Comprueba que un ID ausente genera un error de dominio de tipo no encontrado.</summary>
    [Fact]
    public async Task GetById_ReturnsNotFoundError_WhenRepositoryHasNoFunko()
    {
        var (service, repository, cache) = CreateService();
        repository.Setup(repo => repo.GetByIdAsync(9)).ReturnsAsync((FunkoEntity?)null);
        cache
            .Setup(service => service.GetAsync<Funko>("funkos:9"))
            .ReturnsAsync((Funko?)null);

        var result = await service.GetById(9);

        Assert.True(result.IsFailure);
        Assert.Equal(DomainErrorCode.NotFound, result.Error.Code);
    }

    /// <summary>Comprueba que un identificador inválido no consulta el repositorio.</summary>
    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task GetById_ReturnsValidationError_WhenIdIsInvalid(int id)
    {
        var (service, repository, _) = CreateService();

        var result = await service.GetById(id);

        Assert.Equal(DomainErrorCode.Validation, result.Error.Code);
        repository.Verify(repo => repo.GetByIdAsync(It.IsAny<int>()), Times.Never);
    }

    /// <summary>Comprueba que una entrada individual en caché evita consultar el repositorio.</summary>
    [Fact]
    public async Task GetById_ReturnsCachedFunko_WhenAvailable()
    {
        var (service, repository, cache) = CreateService();
        var funko = CreateValidFunko();
        cache.Setup(item => item.GetAsync<Funko>("funkos:3")).ReturnsAsync(funko);

        var result = await service.GetById(3);

        Assert.True(result.IsSuccess);
        Assert.Same(funko, result.Value);
        repository.Verify(repo => repo.GetByIdAsync(It.IsAny<int>()), Times.Never);
    }

    /// <summary>Comprueba el error de persistencia cuando el repositorio no devuelve el Funko creado.</summary>
    [Fact]
    public async Task Create_ReturnsPersistenceFailure_WhenRepositoryReturnsNull()
    {
        var (service, repository, _) = CreateService();
        repository.Setup(repo => repo.CreateAsync(It.IsAny<FunkoEntity>()))
            .ReturnsAsync((FunkoEntity?)null);

        var result = await service.Create(CreateValidFunko());

        Assert.Equal(DomainErrorCode.PersistenceFailure, result.Error.Code);
    }

    /// <summary>Comprueba el error no encontrado cuando el repositorio no actualiza el Funko.</summary>
    [Fact]
    public async Task Update_ReturnsNotFound_WhenRepositoryReturnsNull()
    {
        var (service, repository, _) = CreateService();
        var funko = CreateValidFunko();
        funko.Id = 3;
        repository.Setup(repo => repo.UpdateAsync(It.IsAny<FunkoEntity>()))
            .ReturnsAsync((FunkoEntity?)null);

        var result = await service.Update(3, funko);

        Assert.Equal(DomainErrorCode.NotFound, result.Error.Code);
    }

    /// <summary>Comprueba que actualizar con el nombre de otro Funko produce un conflicto.</summary>
    [Fact]
    public async Task Update_ReturnsConflict_WhenNameBelongsToAnotherFunko()
    {
        var (service, repository, _) = CreateService();
        repository.Setup(repo => repo.ExistsByNameAsync("Funko", 3)).ReturnsAsync(true);
        var funko = CreateValidFunko();
        funko.Id = 3;

        var result = await service.Update(3, funko);

        Assert.Equal(DomainErrorCode.Conflict, result.Error.Code);
        repository.Verify(repo => repo.UpdateAsync(It.IsAny<FunkoEntity>()), Times.Never);
    }

    /// <summary>Comprueba el error no encontrado cuando el repositorio no elimina el Funko.</summary>
    [Fact]
    public async Task Delete_ReturnsNotFound_WhenRepositoryReturnsNull()
    {
        var (service, repository, _) = CreateService();
        repository.Setup(repo => repo.DeleteAsync(3)).ReturnsAsync((FunkoEntity?)null);

        var result = await service.Delete(3);

        Assert.Equal(DomainErrorCode.NotFound, result.Error.Code);
    }

    /// <summary>Comprueba que eliminar con un identificador inválido devuelve un error de validación.</summary>
    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task Delete_ReturnsValidationError_WhenIdIsInvalid(int id)
    {
        var (service, repository, _) = CreateService();

        var result = await service.Delete(id);

        Assert.Equal(DomainErrorCode.Validation, result.Error.Code);
        repository.Verify(repo => repo.DeleteAsync(It.IsAny<int>()), Times.Never);
    }

    /// <summary>Comprueba que actualizar con un identificador inválido devuelve un error de validación.</summary>
    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task Update_ReturnsValidationError_WhenIdIsInvalid(int id)
    {
        var (service, repository, _) = CreateService();

        var result = await service.Update(id, CreateValidFunko());

        Assert.Equal(DomainErrorCode.Validation, result.Error.Code);
        repository.Verify(repo => repo.UpdateAsync(It.IsAny<FunkoEntity>()), Times.Never);
    }

    /// <summary>Comprueba que un ID de ruta distinto al del modelo falla antes de persistir.</summary>
    [Fact]
    public async Task Update_ReturnsValidationError_WhenRouteAndModelIdsDiffer()
    {
        var (service, repository, _) = CreateService();
        var funko = CreateValidFunko();
        funko.Id = 2;

        var result = await service.Update(1, funko);

        Assert.True(result.IsFailure);
        Assert.Equal(DomainErrorCode.Validation, result.Error.Code);
        repository.Verify(repo => repo.UpdateAsync(It.IsAny<FunkoEntity>()), Times.Never);
    }

    /// <summary>Crea el servicio con dependencias simuladas para aislar cada caso de prueba.</summary>
    private static (FunkoService Service, Mock<IFunkoRepository> Repository, Mock<ICacheService> Cache)
        CreateService()
    {
        var repository = new Mock<IFunkoRepository>();
        var cache = new Mock<ICacheService>();
        cache
            .Setup(service => service.RemoveAsync(It.IsAny<string>()))
            .Returns(Task.CompletedTask);

        return (new FunkoService(cache.Object, repository.Object), repository, cache);
    }

    /// <summary>Construye un Funko que satisface las reglas de validación del servicio.</summary>
    private static Funko CreateValidFunko() => new()
    {
        Nombre = "Funko",
        Imagen = "funko.png",
        Precio = 12.50m,
        Stock = 3,
        Categoria = "Series",
        Proveedor = 1
    };
}
