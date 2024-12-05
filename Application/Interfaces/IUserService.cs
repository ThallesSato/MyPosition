using Application.Dtos.Input;
using Domain.Models;

namespace Application.Interfaces;

public interface IUserService
{
    Task<User?> GetUserByEmailAsync(string email);
    Task<User?> GetUserByEmailLoadedAsync(string email);
    Task<int> GetUserIdByEmailAsync(string email);
}