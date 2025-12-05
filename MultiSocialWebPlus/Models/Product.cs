using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MultiSocialWebPlus.Models
{
    public enum UnitType
    {
        Adet,
        Metre,
        Metrekare,
        Kilogram
    }

    public class Product
    {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public int? CategoryId { get; set; }
        [ForeignKey(nameof(CategoryId))]
        public Category? Category { get; set; }
        public UnitType Unit { get; set; } = UnitType.Adet;
        public decimal UnitPrice { get; set; } = 0m;
        public decimal Stock { get; set; } = 0m;
        public string Notes { get; set; } = "";
    }
}
