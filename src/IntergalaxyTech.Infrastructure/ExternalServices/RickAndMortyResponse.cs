using System.Text.Json.Serialization;

namespace IntergalaxyTech.Infrastructure.ExternalServices;

public class RickAndMortyResponse
{
    [JsonPropertyName("results")]
    public List<RickAndMortyCharacter> Results { get; set; } = new();
}

public class RickAndMortyCharacter
{
    [JsonPropertyName("id")]
    public int Id { get; set; }
    
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;
    
    [JsonPropertyName("status")]
    public string Status { get; set; } = string.Empty;
    
    [JsonPropertyName("species")]
    public string Species { get; set; } = string.Empty;

    [JsonPropertyName("image")]
    public string Image { get; set; } = string.Empty;

    [JsonPropertyName("origin")]
    public RickAndMortyOrigin Origin { get; set; } = new();
}

public class RickAndMortyOrigin
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;
}
