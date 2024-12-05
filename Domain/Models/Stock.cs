
namespace Domain.Models;

public class Stock : BaseEntity
{
    public string Symbol { get; set; } = string.Empty;
    public string? Name { get; set; }
    public int SectorId { get; set; }
    public decimal LastPrice { get; set; }

    public virtual Sector Sector { get; set; } = new();
    public virtual List<StockHistory>? Historicals { get; set; } 
}