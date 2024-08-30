using Domain.Models;

namespace Application.Interfaces;

public interface IPositionService : IBaseService<Positions>
{
    Task<Positions?> GetPositionByWalletAndStockOrDefaultAsync(int walletId, int stockId);
    Task<Positions> GetPositionByWalletAndStockOrCreateAsync(TransactionHistory history, Stock stock);
    Dictionary<DateTime, decimal> GetTotalAmountByDate(Wallet wallet, DateTime? date);
    List<PositionHistory> GetPositionHistoriesAfterDateOrLast(Positions positions, DateTime? date);
}