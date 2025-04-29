using System;
using System.Collections.Generic;

namespace YARDK.Models;

public partial class Product
{
    public int Id { get; set; }

    public string? ProductName { get; set; }

    public string? Description { get; set; }

    public string? Category { get; set; }

    public int? Quantity { get; set; }

    public decimal? Price { get; set; }

    public string? Condition { get; set; }

    public string? ImageUrl { get; set; }

    public string? Status { get; set; }

    public int? SellerId { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual ICollection<CartItem> CartItems { get; set; } = new List<CartItem>();

    public virtual ICollection<FeaturedAd> FeaturedAds { get; set; } = new List<FeaturedAd>();

    public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();

    public virtual ICollection<ProductImage> ProductImages { get; set; } = new List<ProductImage>();

    public virtual User? Seller { get; set; }
}
