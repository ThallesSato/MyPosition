using Application.Dtos.Input;

namespace Application.Interfaces;

public interface IAuthService
{
    Task<bool> Register(UserRegisterDto dto);
    Task<bool> Login(UserLoginDto dto);
    Task<bool> IsEmailInDb(string email);
}