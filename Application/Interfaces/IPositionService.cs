using Domain.Models;

namespace Application.Interfaces;

public interface IPositionService : IBaseService<Positions>
{
    Task<Positions> GetOrCreateAsync(int walletId, int stockId);
}