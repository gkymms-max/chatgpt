using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace MultiSocialWebPlus.Models
{
    public class Quote
    {
        [Key]
        public int Id { get; set; }
        public int CustomerId { get; set; }
        public Customer? Customer { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? ValidUntil { get; set; }
        public decimal Total { get; set; } = 0m;
        public List<QuoteItem> Items { get; set; } = new List<QuoteItem>();
    }
}
