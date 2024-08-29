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
    
    public async Task<List<StockHistory>> GetStockHistoryList(Stock stock, DateTime date)
    {
        var history = await _bovespa.GetStockHistory(stock, date);
        if (history != null)
        {
            await _repository.CreateStockHistoryWithListAsync(history);
        }
        else
        {
            history = await _repository.GetStockHistoryListByStockIdAndDateAsync(stock.Id, date);
        }
        return history;
    }
}