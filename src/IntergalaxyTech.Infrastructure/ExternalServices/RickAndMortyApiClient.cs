using System.Net.Http.Json;
using IntergalaxyTech.Application.Interfaces;
using IntergalaxyTech.Domain.Entities;

namespace IntergalaxyTech.Infrastructure.ExternalServices;

public class RickAndMortyApiClient : IRickAndMortyApiClient
{
    private readonly HttpClient _httpClient;

    public RickAndMortyApiClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<IEnumerable<Personaje>> GetCharactersAsync(int page = 1)
    {
        var response = await _httpClient.GetFromJsonAsync<RickAndMortyResponse>($"?page={page}");

        if (response?.Results == null)
            return Enumerable.Empty<Personaje>();

        return response.Results.Select(r => new Personaje
        {
            Id = r.Id, 
            Nombre = r.Name,
            Especie = r.Species,
            Estado = r.Status,
            Origen = r.Origin?.Name ?? "Unknown",
            Imagen = r.Image
        });
    }
}
