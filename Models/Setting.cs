using System.ComponentModel.DataAnnotations;

namespace SagraPOS.Models
{
    public class Setting
    {
        [Key]
        public string Key { get; set; } = null!;
        public int Category { get; set; }
        public string? ValueString { get; set; } = null!;
        public float? ValueNum { get; set; }
        public byte[]? ValueBlob { get; set; }
    }
}