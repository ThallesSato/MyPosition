namespace Infra.ExternalApi.Dtos;

public class StockApiDto
{
    public string? Symbol { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal LastPrice { get; set; }
    public string Sector { get; set; }  = string.Empty;
}