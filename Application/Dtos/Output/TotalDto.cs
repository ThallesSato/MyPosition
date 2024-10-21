using Domain.Models;

namespace Application.Dtos.Output;

public class TotalDto
{
    public decimal TotalCost { get; set; }
    public decimal TotalValue { get; set; }
    public decimal ResultValue { get; set; }
    public decimal ResultPercentage { get; set; }
    public List<SectorDto> PercentagePerSectors { get; set; } = new();
}