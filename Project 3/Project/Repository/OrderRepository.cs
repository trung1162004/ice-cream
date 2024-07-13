using System;
using Project.Models.Identity;
using Project.Models.ViewModels;
namespace Project.Repository
{
	public interface OrderRepository
    {
        bool UpdateOrderStatus(int orderId, string status);
    }
}

