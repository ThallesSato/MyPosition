using Domain.Models;

namespace Application.Dtos.Output;

public class PositionDto
{

    public int WalletId { get; set; }
    public int StockId { get; set; }
    public int Amount { get; set; }
    public decimal TotalPrice { get; set; }
    public decimal TotalValue { get; set; }
    public decimal Profit { get; set; }
    public decimal ProfitPctg { get; set; }
    public virtual Stock Stock { get; set; } = new();
}