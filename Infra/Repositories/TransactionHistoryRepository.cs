using Application.Dtos.Internal;
using Domain.Models;
using Infra.Context;
using Infra.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Infra.Repositories;

public class TransactionHistoryRepository : BaseRepository<TransactionHistory>, ITransactionHistoryRepository 
{
    private readonly AppDbContext _context;
    
    public TransactionHistoryRepository(AppDbContext context) : base(context)
    {
        _context = context;
    }

    public async Task<List<TotalAmount>?> GetTotalAmountByDateAsync(int walletId)
    {
        return await _context
            .TransactionHistories
            .Where(x => x.WalletId == walletId)
            .GroupBy(x => x.Date)
            .OrderBy(x => x.Key)
            .Select(g => new TotalAmount
            {
                Date = g.Key,
                Amount = g.Sum(x => (double)x.EquityEffect)
            })
            .ToListAsync();
    }
}