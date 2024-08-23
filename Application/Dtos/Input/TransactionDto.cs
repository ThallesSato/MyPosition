namespace Application.Dtos.Input;

public class TransactionDto
{
    public int WalletId { get; set; }
    public string? StockSymbol { get; set; }
    public int Amount { get; set; }
    public decimal Price { get; set; }
    public DateTime Date { get; set; } = DateTime.UtcNow;
}