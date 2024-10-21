using System.Xml;
using Application.Dtos.Input;
using Application.Interfaces;
using Domain.Models;
using Infra.Interfaces;
using Mapster;

namespace Application.Services;

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;

    public AuthService(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<bool> Register(UserRegisterDto dto)
    {
        var user = dto.Adapt<User>();
        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password);
        await _userRepository.RegisterAsync(user);

        return true;
    }

    public async Task<bool> Login(UserLoginDto dto)
    {
        var user = await _userRepository.GetUserByEmailAsync(dto.Email);

        return user != null && BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash);
    }
    
    public async Task<bool> IsEmailInDb(string email)
    {
        return await _userRepository.IsEmailInDbAsync(email);
    }
}