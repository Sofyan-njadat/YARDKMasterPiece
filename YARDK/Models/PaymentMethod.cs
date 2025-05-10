using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace YARDK.Models
{
    public class PaymentMethod
    {
        [Key]
        public int Id { get; set; }
        
        public int UserId { get; set; }
        
        [ForeignKey("UserId")]
        public User User { get; set; }
        
        [Required]
        [StringLength(19)]
        public string CardNumber { get; set; }
        
        [Required]
        [StringLength(255)]
        public string MaskedCardNumber { get; set; }
        
        [Required]
        [StringLength(5)]
        public string ExpiryDate { get; set; }
        
        [Required]
        [StringLength(3)]
        public string CVV { get; set; }
        
        [StringLength(100)]
        public string CardHolderName { get; set; }
        
        [StringLength(20)]
        public string CardType { get; set; }
        
        public bool IsDefault { get; set; }
        
        public DateTime CreatedAt { get; set; }
        
        public DateTime? UpdatedAt { get; set; }
    }
} 