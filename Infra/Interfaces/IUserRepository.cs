using Domain.Models;

namespace Infra.Interfaces;

public interface IUserRepository
{
    Task RegisterAsync(User user);
    Task<User?> GetUserByEmailAsync(string email);
    Task<User?> GetUserByEmailLoadedAsync(string email);
    Task<bool> IsEmailInDbAsync(string email);
    Task<int> GetUserIdByEmailAsync(string email);
}