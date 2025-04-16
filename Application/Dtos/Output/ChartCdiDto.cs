namespace Application.Dtos.Output;

public class ChartCdiDto
{
    public List<string> Dates { get; set; }
    public List<decimal> Variation { get; set; }
    public List<decimal> Cdi { get; set; }
}