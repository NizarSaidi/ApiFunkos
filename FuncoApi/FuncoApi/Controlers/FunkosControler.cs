using FuncoApi.Dto.FunkoDto;
using FuncoApi.Mappers;
using FuncoApi.Services.FunkosService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FuncoApi.Controlers;

/// <summary>Expone las operaciones HTTP CRUD de Funkos.</summary>
[ApiController]
[Route("api/funcopops")]
public class FunkosControler : ApiControllerBase
{
    private readonly IFunkosService _service;

    /// <summary>Inicializa el controlador con el servicio de Funkos.</summary>
    /// <param name="service">Servicio de aplicación de Funkos.</param>
    public FunkosControler(IFunkosService service)
    {
        _service = service;
    }

    /// <summary>Obtiene una página de Funkos y sus metadatos de paginación.</summary>
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

        var pageSize = FuncoApi.Configure.ConfigurationApp.PageSize;
        var data = result.Value.Objs.Select(FunkoMapper.ToResponseDto).ToList();
        return Ok(new
        {
            data,
            currentPage = page,
            pageSize,
            totalCount = result.Value.Count,
            totalPages = (int)Math.Ceiling(result.Value.Count / (double)pageSize)
        });
    }

    /// <summary>Busca un Funko por su identificador.</summary>
    /// <param name="id">Identificador del Funko.</param>
    /// <returns>El Funko o un error de dominio.</returns>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(FunkoResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<FunkoResponseDto>> GetById([FromRoute] int id)
    {
        var result = await _service.GetById(id);
        return result.IsSuccess
            ? Ok(FunkoMapper.ToResponseDto(result.Value))
            : ToHttpError(result.Error);
    }

    /// <summary>Crea un Funko y devuelve el recurso con el ID generado por la base de datos.</summary>
    /// <param name="request">Datos del Funko que se va a crear.</param>
    /// <returns>El Funko creado o un error de dominio.</returns>
    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(FunkoResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<FunkoResponseDto>> Create([FromBody] FunkoCreateDto request)
    {
        var result = await _service.Create(FunkoMapper.ToModel(request));

        if (result.IsFailure)
        {
            return ToHttpError(result.Error);
        }

        var response = FunkoMapper.ToResponseDto(result.Value);
        return Created($"/api/funcopops/{response.Id}", response);
    }

    /// <summary>Actualiza los datos editables de un Funko.</summary>
    /// <param name="id">Identificador del Funko que se va a actualizar.</param>
    /// <param name="request">Nuevos datos del Funko.</param>
    /// <returns>El Funko actualizado o un error de dominio.</returns>
    [HttpPut("{id:int}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(FunkoResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<FunkoResponseDto>> Update([FromRoute] int id, [FromBody] FunkoUpdateDto request)
    {
        var result = await _service.Update(id, FunkoMapper.ToModel(id, request));

        return result.IsSuccess
            ? Ok(FunkoMapper.ToResponseDto(result.Value))
            : ToHttpError(result.Error);
    }

    /// <summary>Elimina un Funko por su identificador.</summary>
    /// <param name="id">Identificador del Funko que se va a eliminar.</param>
    /// <returns>El Funko eliminado o un error de dominio.</returns>
    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(FunkoResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<FunkoResponseDto>> Delete([FromRoute] int id)
    {
        var result = await _service.Delete(id);
        return result.IsSuccess
            ? Ok(FunkoMapper.ToResponseDto(result.Value))
            : ToHttpError(result.Error);
    }
}