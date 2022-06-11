using System.Collections.Generic;

namespace Domain.Services.Autogrouping
{
    public class MoveOrderRequest
    {
        public string NewShippingId { get; set; }
        public List<string> OrderIds { get; set; }
    }
}
