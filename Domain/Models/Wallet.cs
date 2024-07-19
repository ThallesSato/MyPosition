namespace Domain.Models;

public class Wallet : BaseEntity
{
    public required string Name { get; set; }
    public virtual List<Positions> Positions { get; set; } = new();
}   