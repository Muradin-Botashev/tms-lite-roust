using Domain.Persistables;
using System.Collections.Generic;

namespace Application.Services.Autogrouping
{
    public class ShippingRoute
    {
        public decimal PalletsCount { get; set; }
        public decimal WeightKg { get; set; }
        public VehicleType VehicleType { get; set; }
        public CostData FtlCost { get; set; }
        public List<CostData> AllFtlCosts { get; set; }
        public List<PseudoShipping> Shippings { get; set; }
    }
}
