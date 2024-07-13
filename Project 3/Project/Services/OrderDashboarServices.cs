using System;
using Project.Data;
using Project.Models;
using Project.Repository;

namespace Project.Services
{
    public class OrderDashboarServices : OrderDashboardRepository
    {
        private AppDbContext _db;
        public OrderDashboarServices(AppDbContext db) { _db = db; }
        public List<OrderItem> findAll()
        {
            return _db.OrderItems.ToList();
        }
    }
}

