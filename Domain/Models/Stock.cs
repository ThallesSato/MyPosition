
namespace Domain.Models;

public class Stock : BaseEntity
{
    public string? Symbol { get; set; }
    public string? Name { get; set; }
    public int SectorId { get; set; }
    public decimal LastPrice { get; set; }
    
    public virtual Sector? Setor { get; set; } 
}