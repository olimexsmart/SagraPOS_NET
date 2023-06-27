namespace SagraPOS.Models
{
    // TODO separate in another table the image column, some API can be simplified 
    public class MenuEntry
    {
        public int ID { get; set; }
        public int CategoryID { get; set; }
        public string Name { get; set; } = null!;
        public float Price { get; set; }
        public byte[]? Image { get; set; }
        public int Inventory { get; set; }
    }
}