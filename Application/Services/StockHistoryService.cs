using Application.Interfaces;
using Domain.Models;
using Infra.ExternalApi.Interfaces;
using Infra.Interfaces;

namespace Application.Services;

public class StockHistoryService : BaseService<StockHistory>, IStockHistoryService
{
    private readonly IStockHistoryRepository _repository;
    private readonly IBovespa _bovespa;
    public StockHistoryService(IStockHistoryRepository repository, IBovespa bovespa) : base(repository)
    {
        _repository = repository;
        _bovespa = bovespa;
    }
    
    public async Task<List<StockHistory>?> GetStockHistoryListOrCreateAllAsync(Stock stock, DateTime date)
    {
        var stockHistory = await _repository.GetStockHistoryOrDefaultAsync(stock.Id, date);
        if (stockHistory == null && date.DayOfWeek != DayOfWeek.Sunday && date.DayOfWeek != DayOfWeek.Saturday)
        {
            await GenerateStockHistoryForDateAsync(stock, date);
        }
        return _repository.GetStockHistoryList(stock.Id, date);
    }
    public async Task<StockHistory?> GetStockHistoryOrCreateAllAsync(Stock stock, DateTime date)
    {
        if (date.DayOfWeek == DayOfWeek.Sunday || date.DayOfWeek == DayOfWeek.Saturday)
            return null;
        
        var stockHistory = await _repository.GetStockHistoryOrDefaultAsync(stock.Id, date);
        if (stockHistory == null)
        {
            stockHistory = await GenerateStockHistoryForDateAsync(stock, date);
        }
        return stockHistory;
    }
    public async Task<StockHistory?> GenerateStockHistoryForDateAsync(Stock stock, DateTime date)
    {
        StockHistory? stockHistory = null;
        var stockHistoryList = await _bovespa.GetStockHistory(stock, date);
        if (stockHistoryList == null)
            return null;
        foreach (var item in stockHistoryList)
        {
            try
            {
                await _repository.CreateAsync(item);
                if (item.Date == date)
                    stockHistory = item;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }
        return stockHistory;
    }
}