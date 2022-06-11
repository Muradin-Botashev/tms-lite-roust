using DAL.Services;
using Domain.Persistables;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Application.Shared.Shippings
{
    public class DriverDataSyncService: IDriverDataSyncService
    {
        private readonly ICommonDataService _commonDataService;

        public DriverDataSyncService(ICommonDataService commonDataService)
        {
            _commonDataService = commonDataService;
        }

        public void SyncDriverProperties(Shipping shipping, IEnumerable<Order> orders)
        {
            this.SyncProperty(shipping, orders, nameof(Shipping.DriverName));
            this.SyncProperty(shipping, orders, nameof(Shipping.DriverPassportData));
            this.SyncProperty(shipping, orders, nameof(Shipping.DriverPhone));
            this.SyncProperty(shipping, orders, nameof(Shipping.VehicleNumber));
            this.SyncProperty(shipping, orders, nameof(Shipping.VehicleMake));
            this.SyncProperty(shipping, orders, nameof(Shipping.TrailerNumber));
        }

        public void SyncProperty(Shipping shipping, IEnumerable<Order> orders, string propertyName)
        {
            var shippingProp = typeof(Shipping).GetProperty(propertyName);
            var orderProp = typeof(Order).GetProperty(propertyName);

            var uniqueValues = orders.Select(i => orderProp.GetValue(i))
                .Where(i => i != null)
                .Distinct();

            if (uniqueValues.Count() == 1)
            {
                var value = uniqueValues.First();
                orders.ToList().ForEach(i => orderProp.SetValue(i, value));
                shippingProp.SetValue(shipping, value);
            }
            else if (uniqueValues.Count() > 1)
            { 
                orders.ToList().ForEach(i => orderProp.SetValue(i, null));
            }
        }

        public void SetProperty(Shipping shipping, IEnumerable<Order> orders, string propertyName, string propertyValue)
        {
            var shippingProp = typeof(Shipping).GetProperty(propertyName);
            var orderProp = typeof(Order).GetProperty(propertyName);

            orders.ToList().ForEach(i => orderProp.SetValue(i, propertyValue));
            shippingProp.SetValue(shipping, propertyValue);
        }
    }
}
