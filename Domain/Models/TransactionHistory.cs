namespace Domain.Models;

public class TransactionHistory : BaseEntity
{
    public DateTime Date { get; set; }
    public int Amount { get; set; }
    public int WalletId { get; set; }
    public int StockId { get; set; }
    public decimal Price { get; set; }
    
    public virtual Wallet? Wallet { get; set; }
    public virtual Stock? Stock { get; set; }
}