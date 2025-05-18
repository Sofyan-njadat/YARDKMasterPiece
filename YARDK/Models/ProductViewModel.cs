using System;
using System.Collections.Generic;

namespace YARDK.Models
{
    public class ProductViewModel
    {
        public int Id { get; set; }
        public string ProductName { get; set; }
        public string Description { get; set; }
        public string Category { get; set; }
        public int Quantity { get; set; }
        public decimal Price { get; set; }
        public string ImageUrl { get; set; }
        public string SellerName { get; set; }
        public string Status { get; set; }
    }
} 