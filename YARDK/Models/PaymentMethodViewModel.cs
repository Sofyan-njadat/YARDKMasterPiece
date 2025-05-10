using System.ComponentModel.DataAnnotations;

namespace YARDK.Models
{
    public class PaymentMethodViewModel
    {
        [Required(ErrorMessage = "Card number is required")]
        [RegularExpression(@"^[0-9]{16}$", ErrorMessage = "Card number must be 16 digits")]
        [Display(Name = "Card Number")]
        public string CardNumber { get; set; }
        
        [Required(ErrorMessage = "Expiry date is required")]
        [RegularExpression(@"^(0[1-9]|1[0-2])\/([0-9]{2})$", ErrorMessage = "Expiry date must be in MM/YY format")]
        [Display(Name = "Expiry Date")]
        public string ExpiryDate { get; set; }
        
        [Required(ErrorMessage = "CVV is required")]
        [RegularExpression(@"^[0-9]{3}$", ErrorMessage = "CVV must be 3 digits")]
        [Display(Name = "CVV")]
        public string CVV { get; set; }
        
        [Display(Name = "Card Holder Name")]
        public string CardHolderName { get; set; }
    }
} 