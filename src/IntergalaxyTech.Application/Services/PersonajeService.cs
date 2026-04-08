using IntergalaxyTech.Application.DTOs;
using IntergalaxyTech.Application.Interfaces;

namespace IntergalaxyTech.Application.Services;

public class PersonajeService : IPersonajeService
{
    private readonly IRickAndMortyApiClient _apiClient;
    private readonly IRepository<Domain.Entities.Personaje> _personajeRepository;

    public PersonajeService(IRickAndMortyApiClient apiClient, IRepository<Domain.Entities.Personaje> personajeRepository)
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

    public async Task<IEnumerable<PersonajeDto>> ObtenerTodosAsync()
    {
        var personajes = await _personajeRepository.GetAllAsync();
        return personajes.Select(p => new PersonajeDto
        {
            Id = p.Id,
            Nombre = p.Nombre,
            Especie = p.Especie,
            Estado = p.Estado,
            Origen = p.Origen,
            Imagen = p.Imagen
        });
    }
}
