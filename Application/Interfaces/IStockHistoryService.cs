using Domain.Models;

namespace Application.Interfaces;

public interface IStockHistoryService : IBaseService<StockHistory>
{
    Task<StockHistory?> GetStockHistoryOrCreateAllAsync(Stock stock, DateTime date);
    Task<StockHistory?> GenerateStockHistoryForDateAsync(Stock stock, DateTime date);
    Task<List<StockHistory>?> GetStockHistoryListOrCreateAllAsync(Stock stock, DateTime date);
}