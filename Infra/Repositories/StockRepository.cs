using Domain.Models;
using Infra.Context;
using Infra.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Infra.Repositories;

public class StockRepository : BaseRepository<Stock>, IStockRepository
{
    private readonly AppDbContext _context;
    public StockRepository(AppDbContext context) : base(context)
    {
        _context = context;
    }

    public async Task<Stock?> GetStockBySymbolOrDefaultAsync(string symbol)
    {
        return await _context.Stocks.FirstOrDefaultAsync(x => x.Symbol.ToLower() == symbol.ToLower());
    }
}