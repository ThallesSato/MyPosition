using Domain.Models;
using Infra.Context;
using Infra.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Infra.Repositories;

public class StockHistoryRepository : BaseRepository<StockHistory>, IStockHistoryRepository
{
    private readonly AppDbContext _context;
    public StockHistoryRepository(AppDbContext context) : base(context)
    {
        _context = context;
    }
    
    public async Task<StockHistory?> GetStockHistoryOrDefaultAsync(int stockId, DateTime date)
    {
        return await _context
            .StockHistories
            .FirstOrDefaultAsync(x => x.StockId == stockId && x.Date == date);
    }
    
    public List<StockHistory> GetStockHistoryList(int stockId, DateTime date)
    {
        return _context
            .StockHistories
            .Where(x => x.StockId == stockId && x.Date == date)
            .ToList();
    }
}