using System;
using System.Collections.Generic;

namespace YARDK.Models;

public partial class FeaturedAd
{
    public int Id { get; set; }

    public int? ProductId { get; set; }

    public int? SellerId { get; set; }

    public decimal? Price { get; set; }

    public int? Duration { get; set; }

    public DateTime? StartDate { get; set; }

    public DateTime? EndDate { get; set; }

    public string? Status { get; set; }

    public virtual Product? Product { get; set; }

    public virtual User? Seller { get; set; }
}
