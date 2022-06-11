using Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace Domain.Persistables.Queries
{
    public class OrderReport
    {
        public DeliveryType? DeliveryType { get; set; }

        public string ClientName { get; set; }

        public DateTime? ShippingDate { get; set; }

        public int? OrdersCount { get; set; }

        public int? PalletsCount { get; set; }

        public decimal? OrderAmountExcludingVAT { get; set; }

    }
}
