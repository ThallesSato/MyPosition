using Application.Interfaces;
using Domain.Models;
using Infra.ExternalApi.Dtos;
using Infra.Interfaces;
using Mapster;

namespace Application.Services;

public class StockService : BaseService<Stock>, IStockService
{
    private readonly IStockRepository _repository;
    public StockService(IStockRepository repository) : base(repository)
    {
        _repository = repository;
    }

    public async Task<Stock?> CreateStock(StockApiDto stockApiDto, Sector sector)
    {
        var stock = stockApiDto.Adapt<Stock>();
        stock.Setor = sector;
        
        return await _repository.CreateAsync(stock);
    }
    
    public async Task<Stock?> GetStockBySymbolOrDefaultAsync(string symbol)
    {
        return await _repository.GetStockBySymbolOrDefaultAsync(symbol);
    }
}