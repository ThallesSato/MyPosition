using Domain.Models;
using Infra.Context;
using Infra.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Infra.Repositories;

public class StockHistoryRepository : BaseRepository<StockHistory>, IStockHistoryRepository
{
    private readonly DbSet<StockHistory> _context;
    public StockHistoryRepository(AppDbContext context) : base(context)
    {
        _context = context.StockHistories;
    }
    
    public async Task<StockHistory?> GetStockHistoryOrDefaultAsync(int stockId, DateTime date)
    {
        return await _context
            .FirstOrDefaultAsync(x => x.StockId == stockId && x.Date == date);
    }
    
    public List<StockHistory> GetStockHistoryList(int stockId, DateTime date)
    {
        return _context
            .Where(x => x.StockId == stockId && x.Date == date)
            .ToList();
    }
}