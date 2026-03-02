using System;

namespace BizSecureDemo22180044.Models
{
    public class Order
    {
        public int Id { get; set; }
        public int UserId { get; set; } 
        public string Title { get; set; } = "";
        public decimal Amount { get; set; }
        public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    }
}