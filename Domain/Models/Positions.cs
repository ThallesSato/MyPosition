using System.Text.Json.Serialization;

namespace Domain.Models;

public class Positions : BaseEntity
{
    public int WalletId { get; set; }
    public int StockId { get; set; }
    public int Amount { get; set; }
    public decimal TotalPrice { get; set; }
    
    [JsonIgnore]
    public virtual Wallet? Wallet { get; set; }
    public virtual Stock? Stock { get; set; }
    public virtual List<PositionHistory>? PositionHistories { get; set; }
}