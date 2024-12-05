using Domain.Models;
using Infra.Context;
using Infra.Dtos.Internal;
using Infra.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Infra.Repositories;

public class TransactionHistoryRepository : BaseRepository<TransactionHistory>, ITransactionHistoryRepository
{
    private readonly DbSet<TransactionHistory> _context;

    public TransactionHistoryRepository(AppDbContext context) : base(context)
    {
        _context = context.TransactionHistories;
    }

    public async Task<List<TotalAmount>> GetTotalAmountByDateAsync(int walletId)
    {
        return await _context
            .Where(x => x.WalletId == walletId)
            .GroupBy(x => x.Date)
            .OrderBy(x => x.Key)
            .Select(g => new TotalAmount
            {
                Date = g.Key,
                Amount = (decimal)g.Sum(x => (double)x.EquityEffect)    
            })
            .ToListAsync();
    }

    public async Task<List<TransactionHistory>> GetAllByWalletIdAsync(int walletId)
    {
        return await _context
            .Where(x => x.WalletId == walletId)
            .OrderByDescending(x => x.Date)
            .ThenByDescending(x => x.Id)
            .ToListAsync();
    }

    public TransactionHistory? GetFirstByWalletIdOrDefault(int walletId)
    {
        return _context.Where(x => x.WalletId == walletId)
            .OrderBy(x => x.Date)
            .FirstOrDefault();
    }
}