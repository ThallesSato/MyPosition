using Domain.Models;
using Infra.Context;
using Infra.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Infra.Repositories;

public class UserRepository : BaseRepository<User>, IUserRepository 
{
    private readonly DbSet<User> _context;
    public UserRepository(AppDbContext context) : base(context)
    {
        _context = context.Users;
    }
    
    public async Task RegisterAsync(User user)
    {
        await _context.AddAsync(user);
    }
    
    public async Task<User?> GetUserByEmailAsync(string email)
    {
        return await _context
            .FirstOrDefaultAsync(x => x.Email == email);
    }
    public async Task<User?> GetUserByEmailLoadedAsync(string email)
    {
        return await _context
            .Include(x=>x.Wallets)
            .ThenInclude(x=>x.Positions)
            .ThenInclude(x=>x.Stock)
            .FirstOrDefaultAsync(x => x.Email == email);
    }
    
    public async Task<bool> IsEmailInDbAsync(string email)
    {
        return await _context
            .AnyAsync(x => x.Email == email);
    }
}