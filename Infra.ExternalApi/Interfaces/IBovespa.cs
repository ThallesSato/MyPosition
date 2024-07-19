using Domain.Models;
using Infra.ExternalApi.Dtos;

namespace Infra.ExternalApi.Interfaces;

public interface IBovespa
{ 
    Task<(StockApiDto? stock, string? message)> GetStock(string symbol);
}