using Domain.Models;

namespace Infra.ExternalApi.Dtos;

public class StockHistoryDto
{
    public string? Symbol { get; set; }
    public List<StockHistory>? Historicals { get; set; }
}