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
    public async Task<IActionResult> Sync()
    {
        await _personajeService.SyncPersonajesAsync();
        return Ok(new { message = "Sincronización completada." });
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<PersonajeDto>>> Get()
    {
        var personajes = await _personajeService.ObtenerTodosAsync();
        return Ok(personajes);
    }
}
