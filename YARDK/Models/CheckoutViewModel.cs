using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace YARDK.Models
{
    public class CheckoutViewModel
    {
        public List<CartItemViewModel> CartItems { get; set; }
        public decimal Subtotal { get; set; }
        public decimal Shipping { get; set; }
        public decimal Tax { get; set; }
        public decimal Total { get; set; }

        // بيانات المستخدم
        [Required(ErrorMessage = "First name is required")]
        [Display(Name = "First Name")]
        public string FirstName { get; set; }
        
        [Required(ErrorMessage = "Last name is required")]
        [Display(Name = "Last Name")]
        public string LastName { get; set; }
        
        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email address")]
        [Display(Name = "Email")]
        public string Email { get; set; }
        
        [Required(ErrorMessage = "Phone number is required")]
        [Display(Name = "Phone")]
        public string Phone { get; set; }
        
        // بيانات الشحن
        [Required(ErrorMessage = "Address is required")]
        [Display(Name = "Address")]
        public string Address { get; set; }
        
        [Required(ErrorMessage = "City is required")]
        [Display(Name = "City")]
        public string City { get; set; }
        
        [Required(ErrorMessage = "Postal code is required")]
        [Display(Name = "Postal Code")]
        public string PostalCode { get; set; }
        
        // بيانات الدفع
        [Display(Name = "Payment Method")]
        public string PaymentMethod { get; set; }
        
        // Card details (from saved payment method)
        public string CardNumber { get; set; }
        
        public string ExpiryDate { get; set; }
        
        public string CVV { get; set; }
        
        // Flag to use saved payment method
        public bool UseSavedPaymentMethod { get; set; } = true;
        
        // User Id for retrieving data
        public int UserId { get; set; }
        
        // Saved payment method data
        public PaymentMethod SavedPaymentMethod { get; set; }
    }
} 