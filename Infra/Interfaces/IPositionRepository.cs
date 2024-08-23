using Domain.Models;

namespace Infra.Interfaces;

public interface IPositionRepository : IBaseRepository<Positions>
{
    Task<Positions?> GetByWalletAndStockOrDefaultAsync(int walletId, int stockId);
}