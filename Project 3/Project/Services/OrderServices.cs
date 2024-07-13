using System;
using Project.Data;
using Project.Models;
using Project.Repository;

namespace Project.Services
{
    public class OrderServices : OrderRepository
    {
        private AppDbContext _db;
        public OrderServices(AppDbContext db) { _db = db; }
       

        public bool UpdateOrderStatus(int orderId, string status)
        {
            var model = _db.Orders.FirstOrDefault(u => u.OrderId == orderId);
            if (model != null)
            {
                model.Status = status;

                _db.Orders.Update(model);
                _db.SaveChanges();
                return true;
            }
            return false;
        }
    }
    }


