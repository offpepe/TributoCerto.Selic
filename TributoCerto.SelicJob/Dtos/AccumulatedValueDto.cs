namespace TributoCerto.Selic.Dtos;

public class AccumulatedValueDto
{
    public AccumulatedValueDto(DateTime date, decimal value)
    {
        Date = date;
        Value = value;
    }
    public DateTime Date { get; set; }
    public decimal Value { get; set; }
}