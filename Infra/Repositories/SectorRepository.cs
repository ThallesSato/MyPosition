using Domain.Models;
using Infra.Context;
using Infra.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Infra.Repositories;

public class SectorRepository : BaseRepository<Sector>, ISectorRepository
{
    private readonly DbSet<Sector> _context;

    public SectorRepository(AppDbContext context) : base(context)
    {
        _context = context.Sectors;
    }

    public async Task<Sector?> GetSectorByNameAsync(string name)
    {
        return await _context.FirstOrDefaultAsync(x => x.Name == name);
    }
}