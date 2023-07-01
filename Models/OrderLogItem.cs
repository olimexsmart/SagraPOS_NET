using Microsoft.EntityFrameworkCore;

namespace SagraPOS.Models
{
    public class OrderLogItem
    {
        public int ID { get; set; }
        public int OrderID { get; set; }
        public int MenuEntryID { get; set; }
        public int Quantity { get; set; }
        public bool Valid { get; set; }
    }
}