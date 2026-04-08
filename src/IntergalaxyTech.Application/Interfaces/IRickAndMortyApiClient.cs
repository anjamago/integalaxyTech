using IntergalaxyTech.Domain.Entities;

namespace IntergalaxyTech.Application.Interfaces;

public interface IRickAndMortyApiClient
{
    Task<IEnumerable<Personaje>> GetCharactersAsync(int page = 1);
}
