using Application.Dtos.Input;
using Application.Interfaces;
using Domain.Models;
using Infra.Interfaces;

namespace Application.Services;

public class UserService : IUserService
{
    
    private readonly IUserRepository _userRepository;

    public UserService(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<User?> GetUserByEmailAsync(string email)
    {
        return await _userRepository.GetUserByEmailAsync(email);
    }
    public async Task<User?> GetUserByEmailLoadedAsync(string email)
    {
        return await _userRepository.GetUserByEmailLoadedAsync(email);
    }
    
    public async Task<int> GetUserIdByEmailAsync(string email)
    {
        return await _userRepository.GetUserIdByEmailAsync(email);
    }
}