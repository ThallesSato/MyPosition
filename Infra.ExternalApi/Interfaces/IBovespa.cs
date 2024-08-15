using Domain.Models;
using Infra.ExternalApi.Dtos;

namespace Infra.ExternalApi.Interfaces;

public interface IBovespa
{ 
    Task<(StockApiDto? stock, string? message)> GetStock(string symbol);
    Task<decimal?> UpdatePrice(Stock stock);
    Task<List<StockHistory>?> GetStockHistory(Stock stock, DateTime date);
}