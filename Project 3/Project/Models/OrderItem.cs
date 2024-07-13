using System;
using System.Collections.Generic;

namespace Project.Models;

public partial class OrderItem
{
    public int OrderitemId { get; set; }

    public int BookId { get; set; }

    public int OrderId { get; set; }

    public int Quantity { get; set; }

    public decimal? Amount { get; set; }

    public decimal? Total { get; set; }

    public virtual Book? Book { get; set; }

    public virtual Orders? Order { get; set; }
}
