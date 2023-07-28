namespace SagraPOS.Models
{
    public class MenuEntry
    {
        public int ID { get; set; }
        public int CategoryID { get; set; }
        public int PrintCategoryID { get; set; }
        public string Name { get; set; } = null!;
        public float Price { get; set; }
        public byte[]? Image { get; set; }
        public int? Inventory { get; set; }
    }
}