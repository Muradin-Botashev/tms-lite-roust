using Domain.Persistables;

namespace Application.Shared.Distances
{
    public interface IWarehouseDistancesService
    {
        decimal? FindDistance(IMapPoint shippingWarehouse, string shippingCity,
                              IMapPoint deliveryWarehouse, string deliveryCity);
    }
}