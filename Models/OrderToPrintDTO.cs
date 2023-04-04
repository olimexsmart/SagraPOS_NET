namespace SagraPOS.Models;

public class OrderToPrintDTO // TODO unused
{
    public float Total { get; set; } // TODO remove, can be retrieved from DB
    public IEnumerable<OrderEntryDTO> OrderEntries { get; set; } = null!;
}