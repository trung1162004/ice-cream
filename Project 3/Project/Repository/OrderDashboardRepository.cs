using System;
using Project.Models;

namespace Project.Repository
{
	public interface OrderDashboardRepository
	{
        List<OrderItem> findAll();
    }
}

