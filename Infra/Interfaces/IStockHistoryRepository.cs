using Domain.Models;

namespace Infra.Interfaces;

public interface IStockHistoryRepository : IBaseRepository<StockHistory>
{
    Task<StockHistory?> GetStockHistoryOrDefaultAsync(int stockId, DateTime date);
    List<StockHistory> GetStockHistoryList(int stockId, DateTime date);
    
}