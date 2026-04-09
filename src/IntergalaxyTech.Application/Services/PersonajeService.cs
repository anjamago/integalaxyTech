using IntergalaxyTech.Application.DTOs;
using IntergalaxyTech.Application.Interfaces;
using Microsoft.Extensions.Logging;

namespace IntergalaxyTech.Application.Services;

public class PersonajeService : IPersonajeService
{
    private readonly IRickAndMortyApiClient _apiClient;
    private readonly IPersonajeRepository _personajeRepository;
    private readonly ILogger<PersonajeService> _logger;

    public PersonajeService(IRickAndMortyApiClient apiClient, IPersonajeRepository personajeRepository, ILogger<PersonajeService> logger)
    {
        _apiClient = apiClient;
        _personajeRepository = personajeRepository;
        _logger = logger;
    }

    public async Task PersonajesAsync()
    {
        _logger.LogInformation("Iniciando sincronización de personajes desde la API externa.");
        var externalCharacters = await _apiClient.GetCharactersAsync(1);
        var existingCharacters = await _personajeRepository.GetAllAsync();
        var existingIds = existingCharacters.Select(c => c.Id).ToHashSet();

        int cont = 0;
        foreach (var character in externalCharacters)
        {
            if (!existingIds.Contains(character.Id))
            {
                await _personajeRepository.AddAsync(character);
                cont++;
            }
        }
        _logger.LogInformation("Sincronización completada. Se añadieron {Count} personajes nuevos.", cont);
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

    public async Task<PersonajeDto> ObtenerPorIdAsync(int id)
    {
        var personaje = await _personajeRepository.GetByIdAsync(id);
        if (personaje == null)
            throw new KeyNotFoundException($"El personaje con ID {id} no existe en nuestra base de datos importada.");

        return new PersonajeDto
        {
            Id = personaje.Id,
            Nombre = personaje.Nombre,
            Especie = personaje.Especie,
            Estado = personaje.Estado,
            Origen = personaje.Origen,
            Imagen = personaje.Imagen
        };
    }
}
