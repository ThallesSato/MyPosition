﻿namespace Application.Dtos.Input;

public class TransactionDto
{
    public int WalletId { get; set; }
    public int StockId { get; set; }
    public int Amount { get; set; }
    public decimal Price { get; set; }
    public DateTime Date { get; set; } = DateTime.UtcNow;
}