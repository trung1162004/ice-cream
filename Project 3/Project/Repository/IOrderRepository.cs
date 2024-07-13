using Project.Models;

namespace Project.Repository
{
    public interface IOrderRepository
    {
        void SaveOrder(Orders order);
    }
}
