using Domain.Models;
using Infra.Context;
using Infra.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Infra.Repositories;

public class PositionRepository : BaseRepository<Positions>, IPositionRepository
{
    private readonly DbSet<Positions> _context;
    public PositionRepository(AppDbContext context) : base(context)
    {
        _context = context.Positions;
    }

    public Task<Positions?> GetByWalletAndStockOrDefaultAsync(int walletId, int stockId)
    {
        return _context.FirstOrDefaultAsync(x => x.WalletId == walletId && x.StockId == stockId);
    }
}