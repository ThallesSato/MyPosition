using Domain.Models;

namespace Infra.Interfaces;

public interface IPositionRepository : IBaseRepository<Positions>
{
    Task<Positions?> GetByWalletAndStockAsync(int walletId, int stockId);
}