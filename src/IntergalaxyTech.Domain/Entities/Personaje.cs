namespace IntergalaxyTech.Domain.Entities;

public class Personaje
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Especie { get; set; } = string.Empty;
    public string Estado { get; set; } = string.Empty;
    public string Origen { get; set; } = string.Empty;
    public string Imagen { get; set; } = string.Empty;
}
