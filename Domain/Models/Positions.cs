namespace Domain.Models;

public class Positions : BaseEntity
{
    public int WalletId { get; set; }
    public int StockId { get; set; }
    public int Amount { get; set; }
    
    public virtual Wallet Wallet { get; set; }
    public virtual Stock Stock { get; set; }
}