using Domain.Models;

namespace Infra.Interfaces;

public interface IStockHistoryRepository : IBaseRepository<StockHistory>
{
    Task CreateStockHistoryWithListAsync(List<StockHistory> stockHistory);
    Task<List<StockHistory>> GetStockHistoryListByStockIdAndDateAsync(int stockId, DateTime date);
}