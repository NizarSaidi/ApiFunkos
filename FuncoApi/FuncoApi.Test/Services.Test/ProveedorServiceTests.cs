using FuncoApi.Cache.objet;
using FuncoApi.Configure;
using FuncoApi.Domain;
using FuncoApi.Entity;
using FuncoApi.Models;
using FuncoApi.Repositories.proveedoresRepository;
using FuncoApi.Services.ProveedoresService;
using CSharpFunctionalExtensions;
using Moq;

namespace FuncoApi.Test.Services.Test;

/// <summary>Comprueba validación, mapeo y errores del servicio de proveedores.</summary>
public class ProveedorServiceTests
{
    /// <summary>Comprueba que el servicio pasa la página y el tamaño configurado al repositorio.</summary>
    [Fact]
    public async Task GetAll_RequestsPageFromRepository_WhenCacheIsEmpty()
    {
        var (service, repository, cache) = CreateService();
        cache.Setup(item => item.GetAsync<List<Proveedor>>("proveedores:all"))
            .ReturnsAsync((List<Proveedor>?)null);
        repository.Setup(repo => repo.GetAllAsync(2, ConfigurationApp.PageSize))
            .ReturnsAsync(Result.Success<(IReadOnlyList<ProveedorEntity> Objs, int Count), DomainError>(
                (new List<ProveedorEntity> { new() { Id = 20, Nombre = "Página 2" } }, 12)));

        var result = await service.GetAll(2);

        Assert.True(result.IsSuccess);
        var proveedor = Assert.Single(result.Value.Objs);
        Assert.Equal(20, proveedor.Id);
        Assert.Equal("Página 2", proveedor.Nombre);
        Assert.Equal(12, result.Value.Count);
        repository.Verify(repo => repo.GetAllAsync(2, ConfigurationApp.PageSize), Times.Once);
    }

    /// <summary>Comprueba que el servicio propaga el error de persistencia del repositorio.</summary>
    [Fact]
    public async Task GetAll_PropagatesRepositoryDomainError()
    {
        var (service, repository, _) = CreateService();
        var error = Errors.PersistenceFailure("los proveedores");
        repository.Setup(repo => repo.GetAllAsync(1, ConfigurationApp.PageSize))
            .ReturnsAsync(Result.Failure<(IReadOnlyList<ProveedorEntity> Objs, int Count), DomainError>(error));

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

    /// <summary>Comprueba que un proveedor no válido no se envía al repositorio.</summary>
    [Fact]
    public async Task Create_ReturnsValidationError_WithoutCallingRepository()
    {
        var (service, repository, _) = CreateService();

        var result = await service.Create(new Proveedor { Id = 0, Nombre = string.Empty });

        Assert.True(result.IsFailure);
        Assert.Equal(DomainErrorCode.Validation, result.Error.Code);
        repository.Verify(repo => repo.CreateAsync(It.IsAny<ProveedorEntity>()), Times.Never);
    }

    /// <summary>Comprueba el mapeo de entidad y la invalidación de caché al crear un proveedor.</summary>
    [Fact]
    public async Task Create_MapsEntityAndInvalidatesListCache()
    {
        var (service, repository, cache) = CreateService();
        repository
            .Setup(repo => repo.CreateAsync(It.IsAny<ProveedorEntity>()))
            .ReturnsAsync((ProveedorEntity entity) => entity);

        var result = await service.Create(new Proveedor { Id = 5, Nombre = "Distribuidor" });

        Assert.True(result.IsSuccess);
        Assert.Equal(5, result.Value.Id);
        Assert.Equal("Distribuidor", result.Value.Nombre);
        repository.Verify(repo => repo.CreateAsync(It.Is<ProveedorEntity>(entity =>
            entity.Id == 5 && entity.Nombre == "Distribuidor")), Times.Once);
        cache.Verify(service => service.RemoveAsync("proveedores:all"), Times.Once);
        cache.Verify(service => service.RemoveAsync("proveedores:5"), Times.Once);
    }

    /// <summary>Comprueba que no se crea un proveedor cuyo nombre ya está en uso.</summary>
    [Fact]
    public async Task Create_ReturnsConflict_WhenNameAlreadyExists()
    {
        var (service, repository, _) = CreateService();
        repository.Setup(repo => repo.ExistsByNameAsync("Distribuidor", null)).ReturnsAsync(true);

        var result = await service.Create(new Proveedor { Id = 5, Nombre = "Distribuidor" });

        Assert.Equal(DomainErrorCode.Conflict, result.Error.Code);
        Assert.Contains("Distribuidor", result.Error.Message);
        repository.Verify(repo => repo.CreateAsync(It.IsAny<ProveedorEntity>()), Times.Never);
    }

    /// <summary>Comprueba el error de dominio cuando no existe el proveedor solicitado.</summary>
    [Fact]
    public async Task GetById_ReturnsNotFound_WhenProviderDoesNotExist()
    {
        var (service, repository, cache) = CreateService();
        repository.Setup(repo => repo.GetByIdAsync(9)).ReturnsAsync((ProveedorEntity?)null);
        cache
            .Setup(service => service.GetAsync<Proveedor>("proveedores:9"))
            .ReturnsAsync((Proveedor?)null);

        var result = await service.GetById(9);

        Assert.True(result.IsFailure);
        Assert.Equal(DomainErrorCode.NotFound, result.Error.Code);
    }

    /// <summary>Comprueba que un proveedor cacheado evita consultar el repositorio.</summary>
    [Fact]
    public async Task GetById_ReturnsCachedProvider_WhenAvailable()
    {
        var (service, repository, cache) = CreateService();
        var provider = new Proveedor { Id = 5, Nombre = "Distribuidor" };
        cache.Setup(item => item.GetAsync<Proveedor>("proveedores:5")).ReturnsAsync(provider);

        var result = await service.GetById(5);

        Assert.True(result.IsSuccess);
        Assert.Same(provider, result.Value);
        repository.Verify(repo => repo.GetByIdAsync(It.IsAny<int>()), Times.Never);
    }

    /// <summary>Comprueba que IDs no positivos generan errores de validación sin acceder al repositorio.</summary>
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

    /// <summary>Comprueba el error de persistencia cuando no se crea el proveedor.</summary>
    [Fact]
    public async Task Create_ReturnsPersistenceFailure_WhenRepositoryReturnsNull()
    {
        var (service, repository, _) = CreateService();
        repository.Setup(repo => repo.CreateAsync(It.IsAny<ProveedorEntity>()))
            .ReturnsAsync((ProveedorEntity?)null);

        var result = await service.Create(new Proveedor { Id = 5, Nombre = "Distribuidor" });

        Assert.Equal(DomainErrorCode.PersistenceFailure, result.Error.Code);
    }

    /// <summary>Comprueba el error no encontrado cuando el repositorio no actualiza el proveedor.</summary>
    [Fact]
    public async Task Update_ReturnsNotFound_WhenRepositoryReturnsNull()
    {
        var (service, repository, _) = CreateService();
        repository.Setup(repo => repo.UpdateAsync(It.IsAny<ProveedorEntity>()))
            .ReturnsAsync((ProveedorEntity?)null);

        var result = await service.Update(5, new Proveedor { Id = 5, Nombre = "Distribuidor" });

        Assert.Equal(DomainErrorCode.NotFound, result.Error.Code);
    }

    /// <summary>Comprueba que actualizar con el nombre de otro proveedor produce un conflicto.</summary>
    [Fact]
    public async Task Update_ReturnsConflict_WhenNameBelongsToAnotherProvider()
    {
        var (service, repository, _) = CreateService();
        repository.Setup(repo => repo.ExistsByNameAsync("Distribuidor", 5)).ReturnsAsync(true);

        var result = await service.Update(5, new Proveedor { Id = 5, Nombre = "Distribuidor" });

        Assert.Equal(DomainErrorCode.Conflict, result.Error.Code);
        repository.Verify(repo => repo.UpdateAsync(It.IsAny<ProveedorEntity>()), Times.Never);
    }

    /// <summary>Comprueba el error no encontrado cuando el repositorio no elimina el proveedor.</summary>
    [Fact]
    public async Task Delete_ReturnsNotFound_WhenRepositoryReturnsNull()
    {
        var (service, repository, _) = CreateService();
        repository.Setup(repo => repo.DeleteAsync(5)).ReturnsAsync((ProveedorEntity?)null);

        var result = await service.Delete(5);

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

        var result = await service.Update(id, new Proveedor { Id = id, Nombre = "Distribuidor" });

        Assert.Equal(DomainErrorCode.Validation, result.Error.Code);
        repository.Verify(repo => repo.UpdateAsync(It.IsAny<ProveedorEntity>()), Times.Never);
    }

    /// <summary>Comprueba que un ID distinto al del modelo falla antes de persistir.</summary>
    [Fact]
    public async Task Update_ReturnsValidationError_WhenIdsDiffer()
    {
        var (service, repository, _) = CreateService();

        var result = await service.Update(1, new Proveedor { Id = 2, Nombre = "Distribuidor" });

        Assert.True(result.IsFailure);
        Assert.Equal(DomainErrorCode.Validation, result.Error.Code);
        repository.Verify(repo => repo.UpdateAsync(It.IsAny<ProveedorEntity>()), Times.Never);
    }

    /// <summary>Crea el servicio con dependencias simuladas para aislar cada caso de prueba.</summary>
    private static (ProveedorService Service, Mock<IProveedorRepository> Repository, Mock<ICacheService> Cache)
        CreateService()
    {
        var repository = new Mock<IProveedorRepository>();
        var cache = new Mock<ICacheService>();
        cache
            .Setup(service => service.RemoveAsync(It.IsAny<string>()))
            .Returns(Task.CompletedTask);

        return (new ProveedorService(cache.Object, repository.Object), repository, cache);
    }
}
