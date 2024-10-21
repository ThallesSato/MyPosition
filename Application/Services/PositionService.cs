using Application.Interfaces;
using Domain.Models;
using Infra.Interfaces;

namespace Application.Services;

public class PositionService : BaseService<Positions>, IPositionService
{
    private readonly IPositionRepository _repository;
    public PositionService(IPositionRepository repository) : base(repository)
    {
        _repository = repository;
    }

    public async Task<Positions?> GetPositionByWalletAndStockOrDefaultAsync(int walletId, int stockId)
    {
        var position = await _repository.GetByWalletAndStockOrDefaultAsync(walletId, stockId);
        return position;
        
    }

    public async Task<Positions> GetPositionByWalletAndStockOrCreateAsync(TransactionHistory history, Stock stock)
    {
        var position =
            await GetPositionByWalletAndStockOrDefaultAsync(history.WalletId, history.StockId) ??
            new Positions
            {
                WalletId = history.WalletId,
                Stock = stock
            };
        return position;
    }
    
    public SortedDictionary<DateTime, decimal> GetTotalAmountByDate(Wallet wallet, DateTime? date)
    {
        var totalAmountList = new SortedDictionary<DateTime, decimal>();

        foreach (var positions in wallet.Positions)
        {
            var historyList = GetPositionHistoriesAfterDateAndLast(positions, date);
            if (historyList.Count == 0)
                continue;

            foreach (var history in historyList)
            {
                if (totalAmountList.TryGetValue(history.Date.Date, out var currentAmount))
                {
                    totalAmountList[history.Date.Date] = currentAmount + history.TotalPrice;
                }
                else
                {
                    totalAmountList[history.Date.Date] = history.TotalPrice;
                }
            }
        }

        return totalAmountList;
    }
    
    public List<PositionHistory> GetPositionHistoriesAfterDateAndLast(Positions positions,DateTime? date)
    {
        if (date != null)
        {
            var result = positions.PositionHistories
                .Where(x => x.Date.Date > date.Value.Date)
                .OrderBy(x => x.Date)
                .ToList();
            
            var last = positions.PositionHistories.Where(x=>x.Date.Date <= date.Value.Date).MaxBy(x => x.Date);
            
            if (last != null && !result.Any(x=>x.Date.Date < date.Value.Date))
                result.Insert(0,last);
            
            return result;
        }
        else
        {
            var result = positions.PositionHistories
                .OrderBy(x => x.Date)
                .ToList();
            
            return result;
        }
    }
}