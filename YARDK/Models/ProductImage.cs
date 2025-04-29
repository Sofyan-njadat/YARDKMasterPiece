using System;
using System.Collections.Generic;

namespace YARDK.Models;

public partial class ProductImage
{
    public int Id { get; set; }

    public int ProductId { get; set; }

    public string ImageUrl { get; set; }

    public bool IsMainImage { get; set; }

    public int DisplayOrder { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual Product Product { get; set; }
} 