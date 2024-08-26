using Application.Interfaces;
using Domain.Models;
using Infra.ExternalApi.Dtos;
using Infra.ExternalApi.Interfaces;
using Infra.Interfaces;
using Mapster;

namespace Application.Services;

public class StockService : BaseService<Stock>, IStockService
{
    private readonly IStockRepository _repository;
    private readonly IBovespa _bovespa;
    public StockService(IStockRepository repository, IBovespa bovespa) : base(repository)
    {
        _repository = repository;
        _bovespa = bovespa;
    }

    public async Task<Stock?> CreateStockAsync(StockApiDto stockApiDto, Sector sector)
    {
        var stock = stockApiDto.Adapt<Stock>();
        stock.Setor = sector;
        
        return await _repository.CreateAsync(stock);
    }
    
    public async Task<Stock?> GetStockBySymbolOrDefaultAsync(string symbol)
    {
        return await _repository.GetStockBySymbolOrDefaultAsync(symbol);
    }

    public async Task UpdateAllStocksAsync()
    {
        var stocks = await GetAllAsync();
        foreach (var stock in stocks)
        {   
            stock.LastPrice = await _bovespa.UpdatePrice(stock) ?? stock.LastPrice;
            Put(stock);
        }
    }
}