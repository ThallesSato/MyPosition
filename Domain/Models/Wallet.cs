using System.Text.Json.Serialization;

namespace Domain.Models;

public class Wallet : BaseEntity
{
    public required string Name { get; set; }
    public virtual List<Positions> Positions { get; set; } = new();
    [JsonIgnore]
    public virtual User User { get; set; } = new();
}   