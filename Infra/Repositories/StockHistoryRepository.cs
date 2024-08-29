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

    public async Task CreateStockHistoryWithListAsync(List<StockHistory> stockHistory)
    {
        foreach (var item in stockHistory)
        {
            var exists = await _context
                .AnyAsync(x=>x.Date == item.Date && x.StockId == item.StockId);
            if (!exists)
            {
                await _context.AddAsync(item);
            }
        }
    }



    
    public async Task<List<StockHistory>> GetStockHistoryListByStockIdAndDateAsync(int stockId, DateTime date)
    {
        return await _context
            .Where(x => x.StockId == stockId && x.Date >= date)
            .ToListAsync();
    }
}