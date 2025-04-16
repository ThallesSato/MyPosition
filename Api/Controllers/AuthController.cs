using Application.Dtos.Input;
using Application.Interfaces;
using Infra.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly ITokenService _tokenService;
    private readonly IUnitOfWork _unitOfWork;

    public AuthController(IAuthService authService, IUnitOfWork unitOfWork, ITokenService tokenService)
    {
        _authService = authService;
        _unitOfWork = unitOfWork;
        _tokenService = tokenService;
    }

    [HttpPost("register")]
    public async Task<ActionResult> Register(UserRegisterDto dto)
    {
        try
        {
            // Check if the username is already in use
            if (await _authService.IsEmailInDb(dto.Email))
                return BadRequest("Username already in use");

            // Register the user with the provided credentials
            var success = await _authService.Register(dto);
            
            if (!success)
                return BadRequest("Something went wrong");
            
            // Generate a token for the registered user
            var token = _tokenService.GenerateToken(dto.Email);

            // Save changes to the database
            await _unitOfWork.SaveChangesAsync();

            return Ok(token);
        }
        catch (Exception e)
        {
            return BadRequest(e.Message);
        }
    }

    [HttpPost("login")]
    public async Task<ActionResult> Login([FromBody]UserLoginDto dto)
    {
        try
        {
            // Verify if username and password are correct
            if (!await _authService.Login(dto))
                return BadRequest("Wrong username or password");

            // Generate JWT token
            var token = _tokenService.GenerateToken(dto.Email);
            return Ok(token);
        }
        catch (Exception e)
        {
            return BadRequest(e.Message);
        }
    }
}