using Domain.Persistables;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Shared.Shippings
{
    public interface IDriverDataSyncService
    {
        void SyncProperty(Shipping shipping, IEnumerable<Order> orders, string propertyName);
        
        void SyncDriverProperties(Shipping shipping, IEnumerable<Order> orders);

        void SetProperty(Shipping shipping, IEnumerable<Order> orders, string propertyName, string propertyValue);
    }
}
