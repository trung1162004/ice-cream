using System;
namespace Project.Models
{
	public class BookCombined
	{
 
        public virtual Book? Book { get; set; }
        public virtual Orders? Order { get; set; }
        public virtual OrderItem? OrderItem { get; set; }

    }
}

