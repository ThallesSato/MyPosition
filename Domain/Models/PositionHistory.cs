namespace Domain.Models;

public class PositionHistory : BaseEntity
{
    public DateTime Date { get; set; }
    public int Amount { get; set; }
    public decimal TotalPrice { get; set; }
    public int PositionId { get; set; }
    
    public virtual Positions? Position { get; set; }
}