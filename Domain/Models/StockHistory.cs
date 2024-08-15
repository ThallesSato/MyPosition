namespace Domain.Models;

public class StockHistory : BaseEntity
{
    public DateTime Date { get; set; }
    public decimal Close { get; set; }
    public int StockId { get; set; }
}