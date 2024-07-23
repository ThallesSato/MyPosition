using Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace Infra.Context;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }
    
    public DbSet<Wallet> Wallets { get; set; }
    public DbSet<Stock> Stocks { get; set; }
    public DbSet<Positions> Positions { get; set; }
    public DbSet<TransactionHistory> TransactionHistories { get; set; }
    public DbSet<Sector> Sectors { get; set; }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Stock>()
            .HasIndex(x => x.Symbol)
            .IsUnique();

        modelBuilder.Entity<Positions>()
            .HasIndex(x => new { x.WalletId, x.StockId })
            .IsUnique();
    }
}