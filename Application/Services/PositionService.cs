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

    public async Task<Positions> GetOrCreateAsync(int walletId, int stockId)
    {
        var position = await _repository.GetByWalletAndStockAsync(walletId, stockId);
        if (position != null)
            return position;
        
        return await _repository.CreateAsync(new Positions
        {
            WalletId = walletId,
            StockId = stockId
        });
    }
}