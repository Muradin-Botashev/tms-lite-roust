using Domain.Enums;
using Domain.Extensions;

namespace Application.Services.Orders.Import
{
    public class OrdersImportDto
    {
        /// <summary>
        /// Номер накладной
        /// </summary>
        [FieldType(FieldType.Text), OrderNumber(0)]
        public string OrderNumber { get; set; }

        /// <summary>
        /// Номер заказ клиента
        /// </summary>
        [FieldType(FieldType.Text), OrderNumber(1)]
        public string ClientOrderNumber { get; set; }

        /// <summary>
        /// Дата отгрузки
        /// </summary>
        [FieldType(FieldType.Date), OrderNumber(2), DisplayNameKey("shippingDateOnly")]
        public string ShippingDate { get; set; }

        /// <summary>
        /// Время отгрузки
        /// </summary>
        [FieldType(FieldType.Time), OrderNumber(3)]
        public string ShippingTime { get; set; }

        /// <summary>
        /// Дата доставки
        /// </summary>
        [FieldType(FieldType.Date), OrderNumber(4), DisplayNameKey("deliveryDateOnly")]
        public string DeliveryDate { get; set; }

        /// <summary>
        /// Время доставки
        /// </summary>
        [FieldType(FieldType.Time), OrderNumber(5)]
        public string DeliveryTime { get; set; }

        /// <summary>
        /// Склад отгрузки (наименование)
        /// </summary>
        [FieldType(FieldType.Text), OrderNumber(6)]
        public string ShippingWarehouseName { get; set; }

        /// <summary>
        /// Склад доставки (наименование)
        /// </summary>
        [FieldType(FieldType.Text), OrderNumber(7)]
        public string DeliveryWarehouseName { get; set; }

        /// <summary>
        /// Склад доставки (адрес)
        /// </summary>
        [FieldType(FieldType.Text), OrderNumber(8), DisplayNameKey("deliveryAddress")]
        public string DeliveryAddress { get; set; }

        /// <summary>
        /// Плановое количество паллет
        /// </summary>
        [FieldType(FieldType.Number), OrderNumber(9)]
        public decimal? PalletsCount { get; set; }

        /// <summary>
        /// Плановый вес, кг
        /// </summary>
        [FieldType(FieldType.Number), OrderNumber(10)]
        public decimal? WeightKg { get; set; }

        /// <summary>
        /// Объем накладной, см3
        /// </summary>
        [FieldType(FieldType.Number), OrderNumber(11)]
        public decimal? Volume { get; set; }

        /// <summary>
        /// Сумма накладной, без НДС
        /// </summary>
        [FieldType(FieldType.Number), OrderNumber(12)]
        public decimal? OrderAmountExcludingVAT { get; set; }
    }
}
