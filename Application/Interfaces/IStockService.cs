using Domain.Models;
using Infra.ExternalApi.Dtos;

namespace Application.Interfaces;

public interface IStockService : IBaseService<Stock>
{
    Task<Stock?> CreateStockAsync(StockApiDto stockApiDto, Sector sector);
    Task<Stock?> GetStockBySymbolOrDefaultAsync(string symbol);
    Task UpdateAllStocksAsync();
}