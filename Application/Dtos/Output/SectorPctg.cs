namespace Application.Dtos.Output;

public class SectorPctg
{
    public SectorPctg(string? label, decimal value)
    {
        Label = label;
        Value = value;
    }

    public string? Label { get; set; }
    public decimal Value { get; set; }
}