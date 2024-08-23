using Application.Dtos.Internal;
using Domain.Models;

namespace Infra.Interfaces;

public interface ITransactionHistoryRepository : IBaseRepository<TransactionHistory>
{
    Task<List<TotalAmount>?> GetTotalAmountByDateAsync(int walletId);
}