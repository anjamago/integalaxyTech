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

    [HttpPost("sync")]
    public async Task<ActionResult<ApiResponse<object>>> Sync()
    {
        await _personajeService.SyncPersonajesAsync();
        return Ok(ApiResponse<object>.Ok(null, "Sincronización completada con éxito."));
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<IEnumerable<PersonajeDto>>>> Get()
    {
        var personajes = await _personajeService.ObtenerTodosAsync();
        return Ok(ApiResponse<IEnumerable<PersonajeDto>>.Ok(personajes, "Personajes obtenidos correctamente."));
    }
}
