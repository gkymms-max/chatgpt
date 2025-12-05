using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MultiSocialWebPlus.Models
{
    public class QuoteItem
    {
        [Key]
        public int Id { get; set; }
        public int QuoteId { get; set; }
        [ForeignKey(nameof(QuoteId))]
        public Quote? Quote { get; set; }

        public int ProductId { get; set; }
        [ForeignKey(nameof(ProductId))]
        public Product? Product { get; set; }

        public decimal Quantity { get; set; } = 0m;
        public decimal UnitPrice { get; set; } = 0m;
        public decimal LineTotal { get; set; } = 0m;
    }
}
