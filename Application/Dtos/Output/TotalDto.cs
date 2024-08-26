using Domain.Models;

namespace Application.Dtos.Output;

public class TotalDto
{
    public decimal TotalCost { get; set; }
    public decimal TotalValue { get; set; }
    public decimal ResultValue { get; set; }
    public decimal ResultPercentage { get; set; }
    public Dictionary<string, decimal> PercentagePerSectors { get; set; } = new();
    public Wallet? Wallet { get; set; }
}