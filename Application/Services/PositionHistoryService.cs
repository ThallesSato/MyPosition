using Application.Interfaces;
using Domain.Models;
using Infra.Interfaces;

namespace Application.Services;

public class PositionHistoryService : BaseService<PositionHistory>, IPositionHistoryService
{
    private readonly IPositionHistoryRepository _repository;
    public PositionHistoryService(IBaseRepository<PositionHistory> repository, IPositionHistoryRepository repository1) : base(repository)
    {
        _repository = repository1;
    }

    public async Task UpdateAllPositionHistory(TransactionHistory transaction, int positionId)
    {
        
    }
}