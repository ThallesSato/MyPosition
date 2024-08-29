using Domain.Models;

namespace Application.Interfaces;

public interface IPositionHistoryService : IBaseService<PositionHistory>
{
    Task UpdateAllPositionHistory(TransactionHistory transaction, Positions position);
    Task UpdateOrCreatePositionHistory(TransactionHistory transaction, Positions position);
}