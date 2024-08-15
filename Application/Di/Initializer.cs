using Application.Interfaces;
using Application.Services;
using Domain.Models;
using Infra.Context;
using Infra.ExternalApi.Interfaces;
using Infra.ExternalApi.Services;
using Infra.Interfaces;
using Infra.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Application.Di;

public static class Initializer
{
    public static void ConfigureDi(this IServiceCollection services)
    {
        // Bd
        services.AddDbContext<AppDbContext>(o => o.UseSqlite("Data Source = Database"));
        
        // Repositories
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped(typeof(IBaseRepository<>), typeof(BaseRepository<>));
        services.AddScoped<ISectorRepository, SectorRepository>();
        services.AddScoped<IWalletRepository, WalletRepository>();
        services.AddScoped<IPositionRepository, PositionRepository>();
        services.AddScoped<IStockHistoryRepository, StockHistoryRepository>();

        // Services
        services.AddScoped(typeof(IBaseService<>), typeof(BaseService<>));
        services.AddScoped<IStockService, StockService>();
        services.AddScoped<ISectorService, SectorService>();
        services.AddScoped<IWalletService, WalletService>();
        services.AddScoped<IPositionService, PositionService>();
        services.AddScoped<IStockHistoryService, StockHistoryService>();
        
        // External Services
        services.AddScoped<IBovespa, Bovespa>();
    }
    
}