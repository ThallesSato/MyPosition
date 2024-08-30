using Domain.Models;

namespace Infra.Interfaces;

public interface IPositionHistoryRepository : IBaseRepository<PositionHistory>
{
    Task<PositionHistory?> GetPositionHistoryByPositionIdAndDateOrDefault(int positionId, DateTime date);
    Task<List<PositionHistory>> GetPositionHistoryListByPositionIdAndDateOrDefaultAsync(int positionId, DateTime date);
}