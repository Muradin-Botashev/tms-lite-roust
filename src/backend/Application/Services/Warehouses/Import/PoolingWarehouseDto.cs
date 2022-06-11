using Domain.Enums;
using Domain.Extensions;

namespace Application.Services.Warehouses.Import
{
    public class PoolingWarehouseDto
    {
        [FieldType(FieldType.Text), OrderNumber(1)]
        public string PoolingId { get; set; }

        [DisplayNameKey("Warehouse.WarehouseName")]
        [FieldType(FieldType.Text), OrderNumber(2)]
        public string WarehouseName { get; set; }

        [FieldType(FieldType.Text), OrderNumber(3)]
        public string ClientPoolingId { get; set; }

        [DisplayNameKey("Warehouse.Client")]
        [FieldType(FieldType.Text), OrderNumber(4)]
        public string Client { get; set; }

        [FieldType(FieldType.Text), OrderNumber(5)]
        public string Region { get; set; }

        [FieldType(FieldType.Text), OrderNumber(6)]
        public string Address { get; set; }
    }
}
