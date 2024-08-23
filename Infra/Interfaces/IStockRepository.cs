using Domain.Models;

namespace Infra.Interfaces;

public interface IStockRepository : IBaseRepository<Stock>
{
    Task<Stock?> GetStockBySymbolOrDefaultAsync(string symbol);
}