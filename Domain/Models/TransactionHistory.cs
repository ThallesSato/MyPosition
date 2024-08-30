using System.Text.Json.Serialization;

namespace Domain.Models;

public class TransactionHistory : BaseEntity
{
    public DateTime Date { get; set; } = DateTime.UtcNow;
    public int Amount { get; set; }
    public int WalletId { get; set; }
    public int StockId { get; set; }
    public decimal Price { get; set; }
    public decimal EquityEffect { get; set; }
    [JsonIgnore]
    
    public virtual Wallet? Wallet { get; set; }
    [JsonIgnore]
    public virtual Stock Stock { get; set; } = new();
}