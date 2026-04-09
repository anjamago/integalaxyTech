using IntergalaxyTech.Application.DTOs;
using IntergalaxyTech.Application.Interfaces;

namespace IntergalaxyTech.Application.Services;

public class PersonajeService : IPersonajeService
{
    private readonly IRickAndMortyApiClient _apiClient;
    private readonly IPersonajeRepository _personajeRepository;

    public PersonajeService(IRickAndMortyApiClient apiClient, IPersonajeRepository personajeRepository)
    {
        _apiClient = apiClient;
        _personajeRepository = personajeRepository;
    }

    public async Task SyncPersonajesAsync()
    {
        var externalCharacters = await _apiClient.GetCharactersAsync(1);
        var existingCharacters = await _personajeRepository.GetAllAsync();
        var existingIds = existingCharacters.Select(c => c.Id).ToHashSet();

        foreach (var character in externalCharacters)
        {
            if (!existingIds.Contains(character.Id))
            {
                await _personajeRepository.AddAsync(character);
            }
        }
    }

    public async Task<PagedResult<PersonajeDto>> ObtenerTodosAsync(string? nombre, string? estado, int page, int pageSize)
    {
        var result = await _personajeRepository.GetPagedAsync(nombre, estado, page, pageSize);
        
        return new PagedResult<PersonajeDto>
        {
            TotalCount = result.TotalCount,
            Page = page,
            PageSize = pageSize,
            Items = result.Items.Select(p => new PersonajeDto
            {
                Id = p.Id,
                Nombre = p.Nombre,
                Especie = p.Especie,
                Estado = p.Estado,
                Origen = p.Origen,
                Imagen = p.Imagen
            })
        };
    }
}
