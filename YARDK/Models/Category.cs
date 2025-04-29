using System;
using System.Collections.Generic;

namespace YARDK.Models;

public partial class Category
{
    public int Id { get; set; }

    public string? CategoryName { get; set; }
    
    public string? Description { get; set; }
    
    public string? ImageUrl { get; set; }
}
