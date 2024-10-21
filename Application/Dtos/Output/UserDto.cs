
using Domain.Models;

namespace Application.Dtos.Output;

public class UserDto
{
    
    public string? Email { get; set; }
    public string? Name { get; set; }
    public decimal TotalValue { get; set; }
    public decimal TotalCost { get; set; }
    public decimal TotalProfit { get; set; }
    public decimal ProfitPctg { get; set; }
    public virtual List<WalletDto> Wallets { get; set; } = new();
}