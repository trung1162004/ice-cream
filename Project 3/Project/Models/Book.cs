using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Project.Models;

public partial class Book
{
    public int BookId { get; set; }
    [Required]
    public string? Name { get; set; }
    [Required]
    public string? Description { get; set; }
    [Required]
    public decimal? Price { get; set; }
    [Required]
    public decimal? Discount { get; set; }
    [Required]
    public string? Slug { get; set; }
    [Required]
    public int? Quantity { get; set; }

    public string? Images { get; set; }

    public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
}
