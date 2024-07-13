using Project.Models.Identity;
using System;
using System.Collections.Generic;

namespace Project.Models;

public partial class Orders
{
    public int OrderId { get; set; }

    public string? Name { get; set; }

    public string? Phone { get; set; }

    public string? Address { get; set; }

    public string? Email { get; set; }

    public DateTime? OrderDate { get; set; }

    public string? Status { get; set; }

    public string? PaymentMethod { get; set; }
    public string? UserId { get; set; }
    public AppUser? User { get; set; }
    public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
}
