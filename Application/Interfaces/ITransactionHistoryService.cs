using Domain.Models;
using Infra.Dtos.Internal;

namespace Application.Interfaces;

public interface ITransactionHistoryService: IBaseService<TransactionHistory>
{
    Task<List<TotalAmount>?> GetTotalAmountByDateAsync(int walletId);
    Task<List<TransactionHistory>> GetAllByWalletIdAsync(int walletId);
}