using System;

namespace YARDK.Models
{
    public class PaymentViewModel
    {
        public int Id { get; set; }
        public decimal Amount { get; set; }
        public string PaymentMethod { get; set; }
        public string PaymentStatus { get; set; }
        public string TransactionId { get; set; }
        public DateTime PaymentDate { get; set; }
        public DateTime? LastUpdated { get; set; }
    }
} 