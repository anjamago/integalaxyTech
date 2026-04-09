using IntergalaxyTech.Application.DTOs;

namespace IntergalaxyTech.Application.Interfaces;

public interface IPersonajeService
{
    Task SyncPersonajesAsync();
    Task<PagedResult<PersonajeDto>> ObtenerTodosAsync(string? nombre, string? estado, int page, int pageSize);
}
