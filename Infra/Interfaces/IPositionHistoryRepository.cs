using Domain.Models;

namespace Infra.Interfaces;

public interface IPositionHistoryRepository : IBaseRepository<PositionHistory>
{
    Task<PositionHistory?> GetPositionHistoryByPositionIdAndDateOrDefaultAsync(int positionId, DateTime date);
    Task<List<PositionHistory>> GetPositionHistoryListByPositionIdAndDateOrDefaultAsync(int positionId, DateTime date);
}