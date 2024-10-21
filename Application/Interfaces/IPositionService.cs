using Domain.Models;

namespace Application.Interfaces;

public interface IPositionService : IBaseService<Positions>
{
    Task<Positions?> GetPositionByWalletAndStockOrDefaultAsync(int walletId, int stockId);
    Task<Positions> GetPositionByWalletAndStockOrCreateAsync(TransactionHistory history, Stock stock);
    SortedDictionary<DateTime, decimal> GetTotalAmountByDate(Wallet wallet, DateTime? date);
    List<PositionHistory> GetPositionHistoriesAfterDateAndLast(Positions positions, DateTime? date);
}