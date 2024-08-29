using Domain.Models;

namespace Application.Interfaces;

public interface IStockHistoryService : IBaseService<StockHistory>
{
    /// <summary>
    /// Get stock history from service and update, or get from database
    /// </summary>
    /// <param name="stock">The stock to retrieve history for.</param>
    /// <param name="date">The date to retrieve history newer than.</param>
    /// <returns>A list of stock history entries that are newer than the specified date.</returns>
    Task<List<StockHistory>> GetStockHistoryList(Stock stock, DateTime date);
}