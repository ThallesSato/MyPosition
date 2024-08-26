using Domain.Models;
using Infra.Dtos.Internal;

namespace Infra.Interfaces;

public interface ITransactionHistoryRepository : IBaseRepository<TransactionHistory>
{
    Task<List<TotalAmount>?> GetTotalAmountByDateAsync(int walletId);
    Task<List<IGrouping<int, TransactionHistory>>?> GetAllByWalletIdAsync(int walletId);
}