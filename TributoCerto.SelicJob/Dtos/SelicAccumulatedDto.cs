
namespace TributoCerto.Selic.Dtos;

public class SelicAccumulatedDto
{
    public int Id { get; set; }
    public Guid Uu_Id { get; set; }
    public DateTime Date { get; set; }
    public decimal Value { get; set; }
    public DateTime Payment_Date { get; set; }
    public decimal Accumulated_Value { get; set; }
}