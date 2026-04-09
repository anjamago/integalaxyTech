namespace IntergalaxyTech.Application.DTOs;

public class ReporteSolicitudesDto
{
    public Dictionary<string, int> TotalesPorEstado { get; set; } = new Dictionary<string, int>();
    public string? PersonajeMasSolicitado { get; set; }
}
