using System.Text.Json.Serialization;

namespace Domain.Models;

public class User : BaseEntity
{
    public string? Email { get; set; }
    [JsonIgnore]
    public string? PasswordHash { get; set; }
    public string? Name { get; set; }
    
    public virtual List<Wallet> Wallets { get; set; } = new();
    
}