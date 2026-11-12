using FuncoApi.Configure;
using FuncoApi.Dto.ProveedorDto;
using FuncoApi.Mappers;
using FuncoApi.Services.ProveedoresService;
using Microsoft.AspNetCore.Mvc;

namespace FuncoApi.Controlers;

/// <summary>Expone las operaciones HTTP CRUD de proveedores.</summary>
[ApiController]
[Tags("Proveedores")]
[Route("api/proveedores")]
public class ProveedoresControler : ApiControllerBase
{
    private readonly IProveedorService _service;

    /// <summary>Inicializa el controlador con el servicio de proveedores.</summary>
    /// <param name="service">Servicio de aplicación de proveedores.</param>
    public ProveedoresControler(IProveedorService service)
    {
        _service = service;
    }

    /// <summary>Obtiene una página de proveedores y sus metadatos de paginación.</summary>
    /// <param name="page">Página solicitada, empezando por 1.</param>
    /// <returns>La página, el total y los metadatos o un error de dominio.</returns>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetAll([FromQuery] int page = 1)
    {
        var result = await _service.GetAll(page);
        if (result.IsFailure)
        {
            return ToHttpError(result.Error);
        }

        var pageSize = ConfigurationApp.PageSize;
        return Ok(new
        {
            data = result.Value.Objs.Select(ProveedorMapper.ToResponseDto).ToList(),
            currentPage = page,
            pageSize,
            totalCount = result.Value.Count,
            totalPages = (int)Math.Ceiling(result.Value.Count / (double)pageSize)
        });
    }

    /// <summary>Busca un proveedor por su identificador.</summary>
    /// <param name="id">Identificador del proveedor.</param>
    /// <returns>El proveedor o un error de dominio.</returns>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ProveedorResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ProveedorResponseDto>> GetById(int id)
    {
        var result = await _service.GetById(id);
        return result.IsSuccess
            ? Ok(ProveedorMapper.ToResponseDto(result.Value))
            : ToHttpError(result.Error);
    }

    /// <summary>Crea un proveedor y devuelve el ID generado por MongoDB.</summary>
    /// <param name="request">Datos del proveedor que se va a crear.</param>
    /// <returns>El proveedor creado o un error de dominio.</returns>
    [HttpPost]
    [ProducesResponseType(typeof(ProveedorResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ProveedorResponseDto>> Create(ProveedorCreateDto request)
    {
        var result = await _service.Create(ProveedorMapper.ToModel(request));
        if (result.IsFailure)
        {
            return ToHttpError(result.Error);
        }

        var response = ProveedorMapper.ToResponseDto(result.Value);
        return Created($"/api/proveedores/{response.Id}", response);
    }

    /// <summary>Actualiza el nombre de un proveedor existente.</summary>
    /// <param name="id">Identificador del proveedor que se va a actualizar.</param>
    /// <param name="request">Nuevo nombre.</param>
    /// <returns>El proveedor actualizado o un error de dominio.</returns>
    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(ProveedorResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ProveedorResponseDto>> Update(int id, ProveedorUpdateDto request)
    {
        var result = await _service.Update(id, ProveedorMapper.ToModel(id, request));
        return result.IsSuccess
            ? Ok(ProveedorMapper.ToResponseDto(result.Value))
            : ToHttpError(result.Error);
    }

    /// <summary>Elimina un proveedor por su identificador.</summary>
    /// <param name="id">Identificador del proveedor que se va a eliminar.</param>
    /// <returns>El proveedor eliminado o un error de dominio.</returns>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(typeof(ProveedorResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ProveedorResponseDto>> Delete(int id)
    {
        var result = await _service.Delete(id);
        return result.IsSuccess
            ? Ok(ProveedorMapper.ToResponseDto(result.Value))
            : ToHttpError(result.Error);
    }
}