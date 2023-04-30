namespace SagraPOS.Models;

public class InfoOrdersDTO
{
    private List<InfoOrderEntry> infoOrderEntries;
    public IEnumerable<InfoOrderEntry> InfoOrderEntries { get => infoOrderEntries; }
    public float OrdersTotal { get; set; }
    public int NumOrders { get; set; }
    
    public InfoOrdersDTO() => infoOrderEntries = new List<InfoOrderEntry>();
    public void AddEntry(InfoOrderEntry ioe) => infoOrderEntries.Add(ioe);

    public class InfoOrderEntry
    {
        public string MenuEntryName { get; set; } = null!;
        public int QuantitySold { get; set; }
        public float TotalSold { get; set; }
        public float TotalSoldPercentage { get; set; }
        public float TotalPercentage { get; set; }
    }
}