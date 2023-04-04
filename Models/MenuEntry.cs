namespace SagraPOS.Models
{
    // TODO separate in another table the image column, some API can be simplified 
    public class MenuEntry
    {
        public int ID { get; set; }
        public int categoryID { get; set; }
        public string name { get; set; } = null!;
        public float price { get; set; }
        public byte[]? image {get; set;}
    }
}