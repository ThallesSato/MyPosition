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

    public virtual Stock Stock { get; set; } = new();
    [JsonIgnore] 
    public virtual List<PositionHistory> PositionHistories { get; set; } = new();


    public List<PositionHistory> GetPositionHistoriesAfterDateOrLast(DateTime? date)
    {
        if (date != null)
        {
            var result = PositionHistories
                .Where(x => x.Date.Date >= date.Value.Date)
                .OrderBy(x => x.Date)
                .ToList();

            if (result.Count > 0)
                return result;
            
            var last = PositionHistories.Where(x=>x.Date.Date <= date.Value.Date).MaxBy(x => x.Date);
            
            if (last != null)
                result.Add(last);
            
            return result;
        }
        else
        {
            var result = PositionHistories
                .OrderBy(x => x.Date)
                .ToList();
            
            return result;
        }
    }
}