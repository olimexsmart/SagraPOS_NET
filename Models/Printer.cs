namespace SagraPOS.Models
{
    public class Printer
    {
        public int ID { get; set; }
        public string Name { get; set; } = null!;
        public string IP { get; set; } = null!;
        public int Port { get; set; }
        public bool Hidden { get; set; }
    }
}