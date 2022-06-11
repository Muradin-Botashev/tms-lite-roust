using Domain.Persistables;

namespace Application.Shared.Orders
{
    public interface IOrderFieldsSyncService
    {
        void SyncWithDeliveryWarehouse(Order order, Warehouse warehouse);
    }
}