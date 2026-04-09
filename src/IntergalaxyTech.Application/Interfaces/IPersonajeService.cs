using IntergalaxyTech.Application.DTOs;

namespace IntergalaxyTech.Application.Interfaces;

public interface IPersonajeService
{
    Task PersonajesAsync();
    Task<PagedResult<PersonajeDto>> ObtenerTodosAsync(string? nombre, string? estado, int page, int pageSize);
    Task<PersonajeDto> ObtenerPorIdAsync(int id);
}
