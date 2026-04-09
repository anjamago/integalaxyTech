using IntergalaxyTech.Application.DTOs;
using IntergalaxyTech.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace IntergalaxyTech.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PersonajesController : ControllerBase
{
    private readonly IPersonajeService _personajeService;

    public PersonajesController(IPersonajeService personajeService)
    {
        _personajeService = personajeService;
    }

    [HttpPost("importar")]
    public async Task<ActionResult<ApiResponse<object>>> Importar()
    {
        await _personajeService.SyncPersonajesAsync();
        return Ok(ApiResponse<object>.Ok(null, "Sincronización de personajes completada con éxito."));
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<PagedResult<PersonajeDto>>>> Get(
        [FromQuery] string? nombre, 
        [FromQuery] string? estado, 
        [FromQuery] int page = 1, 
        [FromQuery] int pageSize = 10)
    {
        var personajes = await _personajeService.ObtenerTodosAsync(nombre, estado, page, pageSize);
        return Ok(ApiResponse<PagedResult<PersonajeDto>>.Ok(personajes, "Personajes obtenidos correctamente."));
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ApiResponse<PersonajeDto>>> GetById(int id)
    {
        var personaje = await _personajeService.ObtenerPorIdAsync(id);
        return Ok(ApiResponse<PersonajeDto>.Ok(personaje, "Personaje recuperado correctamente."));
    }
}
