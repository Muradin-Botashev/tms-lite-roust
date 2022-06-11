namespace Domain.Services.Shippings
{
    public class OrderCostDto
    {
        public string Id { get; set; }
        public string OrderNumber { get; set; }
        public decimal? ReturnCostWithoutVAT { get; set; }
    }
}
