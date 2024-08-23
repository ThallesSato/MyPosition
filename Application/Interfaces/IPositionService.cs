using Domain.Models;

namespace Application.Interfaces;

public interface IPositionService : IBaseService<Positions>
{
    Task<Positions?> GetPositionByWalletAndStockOrDefaultAsync(int walletId, int stockId);
}