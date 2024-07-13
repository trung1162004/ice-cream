using System;
using Project.Models.Identity;
using Project.Models.ViewModels;
namespace Project.Models
{
    // DashboardViewModel.cs
    public class DashboardViewModel
    {
        public List<AppUser> Users { get; set; }
        public List<OrderItem> OrderItems { get; set; }
        public List<Feedback> Feedbacks { get; set; }
    
    }

}

