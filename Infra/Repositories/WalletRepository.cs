using Domain.Models;
using Infra.Context;
using Infra.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Infra.Repositories;

public class WalletRepository : BaseRepository<Wallet>, IWalletRepository
{
    private readonly DbSet<Wallet> _context;
    public WalletRepository(AppDbContext context) : base(context)
    {
        _context = context.Wallets;
    }

    public new async Task<Wallet?> GetByIdOrDefaultAsync(int id)
    {
        return await _context
            .Include(w => w.Positions
                .Where(p=>p.Amount > 0))
            .ThenInclude(p => p.Stock.Setor)
            .FirstOrDefaultAsync(x => x.Id == id);
    }
}