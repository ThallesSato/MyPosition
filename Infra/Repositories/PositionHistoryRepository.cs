using Domain.Models;
using Infra.Context;
using Infra.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Infra.Repositories;

public class PositionHistoryRepository : BaseRepository<PositionHistory>, IPositionHistoryRepository
{
    private readonly DbSet<PositionHistory> _context;
    public PositionHistoryRepository(AppDbContext context) : base(context)
    {
        _context = context.PositionHistories;
    }

    public async Task<PositionHistory?> GetPositionHistoryByPositionIdAndDateOrDefault(int positionId, DateTime date)
    {
        return await _context
            .Where(x => x.PositionId == positionId && x.Date <= date)
            .OrderByDescending(x => x.Date)
            .FirstOrDefaultAsync();
    }
    public async Task<List<PositionHistory>> GetPositionHistoryListByPositionIdAndDateOrDefaultAsync(int positionId, DateTime date)
    {
        return await _context
            .Where(x => x.PositionId == positionId && x.Date > date)
            .ToListAsync();
    }
    
}