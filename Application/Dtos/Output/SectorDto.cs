namespace Application.Dtos.Output;

public class SectorDto
{

    public string? Name { get; set; }
    public decimal Percentage { get; set; }
    public decimal TotalPrice { get; set; }
    public decimal TotalValue { get; set; }
    public decimal Profit { get; set; }
    public decimal ProfitPctg { get; set; }
    public List<PositionDto> Positions { get; set; } = new();
}