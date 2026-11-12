using FuncoApi.Configure;
using FuncoApi.Dto.CategoriaDto;
using FuncoApi.Mappers;
using FuncoApi.Services.CategoriaService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FuncoApi.Controlers;

/// <summary>Expone las operaciones HTTP CRUD de categorías.</summary>
[ApiController]
[Route("api/categorias")]
public class CategoriasControler : ApiControllerBase
{
    private readonly ICategoriaService _service;

    /// <summary>Inicializa el controlador con el servicio de categorías.</summary>
    /// <param name="service">Servicio de aplicación de categorías.</param>
    public CategoriasControler(ICategoriaService service)
    {
        _service = service;
    }

    /// <summary>Obtiene una página de categorías y sus metadatos de paginación.</summary>
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

        var totalCount = result.Value.Count;
        var pageSize = ConfigurationApp.PageSize;
        return Ok(new
        {
            data = result.Value.Objs.Select(CategoriaMapper.ToResponseDto).ToList(),
            currentPage = page,
            totalCount,
            pageSize,
            totalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
        });
    }

    /// <summary>Busca una categoría por su identificador.</summary>
    /// <param name="id">Identificador textual de la categoría.</param>
    /// <returns>La categoría o un error de dominio.</returns>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(CategoriaResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CategoriaResponseDto>> GetById(string id)
    {
        var result = await _service.GetById(id);
        return result.IsSuccess
            ? Ok(CategoriaMapper.ToResponseDto(result.Value))
            : ToHttpError(result.Error);
    }

    /// <summary>Crea una categoría y devuelve el ID generado por la base de datos.</summary>
    /// <param name="request">Datos de la categoría que se va a crear.</param>
    /// <returns>La categoría creada o un error de dominio.</returns>
    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(CategoriaResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<CategoriaResponseDto>> Create([FromBody]CategoriaCreateDto request)
    {
        var result = await _service.Create(CategoriaMapper.ToModel(request));

        if (result.IsFailure)
        {
            return ToHttpError(result.Error);
        }

        var response = CategoriaMapper.ToResponseDto(result.Value);
        return Created($"/api/categorias/{response.Id}", response);
    }

    /// <summary>Actualiza el nombre de una categoría existente.</summary>
    /// <param name="id">Identificador de la categoría que se va a actualizar.</param>
    /// <param name="request">Nuevo nombre.</param>
    /// <returns>La categoría actualizada o un error de dominio.</returns>
    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(CategoriaResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<CategoriaResponseDto>> Update([FromRoute] string id, [FromBody] CategoriaUpdateDto request)
    {
        var result = await _service.Update(id, CategoriaMapper.ToModel(id, request));
        return result.IsSuccess
            ? Ok(CategoriaMapper.ToResponseDto(result.Value))
            : ToHttpError(result.Error);
    }

    /// <summary>Elimina una categoría por su identificador.</summary>
    /// <param name="id">Identificador de la categoría que se va a eliminar.</param>
    /// <returns>La categoría eliminada o un error de dominio.</returns>
    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(CategoriaResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<CategoriaResponseDto>> Delete([FromRoute] string id)
    {
        var result = await _service.Delete(id);
        return result.IsSuccess
            ? Ok(CategoriaMapper.ToResponseDto(result.Value))
            : ToHttpError(result.Error);
    }
}
