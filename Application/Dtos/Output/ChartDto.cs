namespace Application.Dtos.Output;

public class ChartDto
{
    public string? Name { get; set; }
    public List<decimal> Values { get; set; } = new();
}