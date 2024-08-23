using Application.Dtos.Internal;
using Application.Interfaces;
using Domain.Models;
using Infra.Interfaces;

namespace Application.Services;

public class TransactionHistoryService : BaseService<TransactionHistory>, ITransactionHistoryService
{
    private readonly ITransactionHistoryRepository _repository;
    public TransactionHistoryService(IBaseRepository<TransactionHistory> repository, ITransactionHistoryRepository repository1) : base(repository)
    {
        _repository = repository1;
    }

    public async Task<List<TotalAmount>?> GetTotalAmountByDateAsync(int walletId)
    {
        return await _repository.GetTotalAmountByDateAsync(walletId);
    }
}