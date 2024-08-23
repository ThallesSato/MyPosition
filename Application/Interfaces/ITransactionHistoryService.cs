using Application.Dtos.Internal;
using Domain.Models;

namespace Application.Interfaces;

public interface ITransactionHistoryService: IBaseService<TransactionHistory>
{
    Task<List<TotalAmount>?> GetTotalAmountByDateAsync(int walletId);
}