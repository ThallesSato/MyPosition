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

    public async Task UpdateAllPositionHistory(TransactionHistory transaction, Positions positions)
    {
        var positionHistoryList = await _repository.GetPositionHistoryListByPositionIdAndDateOrDefaultAsync(positions.Id, transaction.Date);
        foreach (var positionHistory in positionHistoryList)
        {
            positionHistory.Amount += transaction.Amount;
            positionHistory.TotalPrice += transaction.EquityEffect;
            Put(positionHistory);
        }
    }
    public async Task UpdateOrCreatePositionHistory(TransactionHistory transaction, Positions positions)
    {
        var positionHistory = await _repository.GetPositionHistoryByPositionIdAndDateOrDefault(positions.Id, transaction.Date);
        if (positionHistory == null)
        {
            // If it doesn't exist yet, create a new one
            var positionHistoryDto = new PositionHistory
            {
                Position = positions,
                Date = transaction.Date,
                Amount = transaction.Amount,
                TotalPrice = transaction.EquityEffect
            };
            await _repository.CreateAsync(positionHistoryDto);
        }
        else
        {
            if (positionHistory.Date.Date < transaction.Date.Date)
            {
                // If exists an old one (old date), create a new one updated
                var positionHistoryDto = new PositionHistory
                {
                    Position = positions,
                    Date = transaction.Date.Date,
                    Amount = transaction.Amount + positionHistory.Amount,
                    TotalPrice = transaction.EquityEffect + positionHistory.TotalPrice
                };
                await _repository.CreateAsync(positionHistoryDto);
            }
            else
            {
                // If exists an old one (same date), update it
                positionHistory.Amount += transaction.Amount;
                positionHistory.TotalPrice += transaction.EquityEffect;

                Put(positionHistory);
            }
        }
    }
}