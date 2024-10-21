using System.ComponentModel.DataAnnotations;

namespace Application.Dtos.Input;

public class UserRegisterDto
{
    [Required]
    [EmailAddress]
    public required string Email { get; set; }
    [Required]
    [MinLength(4)]
    public required string Password { get; set; }
    [Required]
    [MinLength(4)]
    public required string Name { get; set; }
}