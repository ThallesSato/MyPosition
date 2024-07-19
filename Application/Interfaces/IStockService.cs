using Domain.Models;
using Infra.ExternalApi.Dtos;

namespace Application.Interfaces;

public interface IStockService : IBaseService<Stock>
{
    Task<Stock?> CreateStock(StockApiDto stockApiDto, Sector sector);
}