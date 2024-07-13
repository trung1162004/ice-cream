using Project.Data;
using Project.Models;
using Project.Repository;

namespace Project.Services
{
    public class OrderService : IOrderRepository
    {
        private AppDbContext _context;
        public OrderService(AppDbContext context) 
        { 
            _context = context;
        }
        public void SaveOrder(Orders order)
        {
            _context.Orders.Add(order);
            _context.SaveChanges();
            // Lưu các OrderItem
            foreach (var orderItem in order.OrderItems)
            {
                // Gán OrderId cho OrderItem
                orderItem.OrderId = order.OrderId;

                // Thêm OrderItem vào context
                _context.OrderItems.Add(orderItem);
            }

        }
    }
}
