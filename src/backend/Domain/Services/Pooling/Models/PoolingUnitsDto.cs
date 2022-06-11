namespace Domain.Services.Pooling.Models
{
    public class PoolingUnitsDto
    {
        public int? PositionFrom { get; set; }

        public int? PositionTo { get; set; }

        public int? Pallets { get; set; }

        public int? Boxes { get; set; }

        public decimal? Weight { get; set; }

        public decimal? Cost { get; set; }

        public decimal? Volume { get; set; }

        public decimal? Length { get; set; }

        public decimal? Width { get; set; }

        public decimal? Height { get; set; }
    }
}
