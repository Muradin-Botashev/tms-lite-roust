using System;
using Domain.Enums;
using Domain.Extensions;

namespace Domain.Persistables
{   
    /// <summary>
    /// Тариф
    /// </summary>
    public class Tariff : IPersistable
    {
        /// <summary>
        /// Db primary key
        /// </summary>    
        public Guid Id { get; set; }

        /// <summary>
        /// Регион отгрузки
        /// </summary>
        public string ShipmentRegion { get; set; }
        /// <summary>
        /// Регион доставки
        /// </summary>
        public string DeliveryRegion { get; set; }

        /// <summary>
        /// Город отгрузки
        /// </summary>
        public string ShipmentCity { get; set; }
        /// <summary>
        /// Город доставки
        /// </summary>
        public string DeliveryCity { get; set; }

        /// <summary>
        /// Склад отгрузки
        /// </summary>
        [ReferenceType(typeof(ShippingWarehouse))]
        public Guid? ShippingWarehouseId { get; set; }

        [SortKey(nameof(Persistables.ShippingWarehouse.WarehouseName))]
        public ShippingWarehouse ShippingWarehouse { get; set; }

        /// <summary>
        /// Склад доставки
        /// </summary>
        [ReferenceType(typeof(Warehouse))]
        public Guid? DeliveryWarehouseId { get; set; }

        [SortKey(nameof(Warehouse.WarehouseName))]
        public Warehouse DeliveryWarehouse { get; set; }

        /// <summary>
        /// Способ тарификации
        /// </summary>
        public TarifficationType? TarifficationType { get; set; }

        /// <summary>
        /// Транспортная компания
        /// </summary>
        [ReferenceType(typeof(TransportCompany))]
        public Guid? CarrierId { get; set; }

        [SortKey(nameof(TransportCompany. Title))]
        public TransportCompany Carrier { get; set; }

        /// <summary>
        /// Тип ТС
        /// </summary>
        [ReferenceType(typeof(VehicleType))]
        public Guid? VehicleTypeId { get; set; }

        [SortKey(nameof(Persistables.VehicleType.Name))]
        public VehicleType VehicleType { get; set; }

        /// <summary>
        /// Тип Кузова
        /// </summary>
        [ReferenceType(typeof(BodyType))]
        public Guid? BodyTypeId { get; set; }

        [SortKey(nameof(Persistables.BodyType.Name))]
        public BodyType BodyType { get; set; }

        /// <summary>
        /// Начало зимнего периода
        /// </summary>
        public DateTime? StartWinterPeriod { get; set; }

        /// <summary>
        /// Окончание зимнего периода
        /// </summary>
        public DateTime? EndWinterPeriod { get; set; }

        /// <summary>
        /// Зимняя надбавка
        /// </summary>
        public decimal? WinterAllowance { get; set; }

        /// <summary>
        /// Дата начала действия тарифов
        /// </summary>
        public DateTime? EffectiveDate { get; set; }

        /// <summary>
        /// Дата окончания действия тарифов
        /// </summary>
        public DateTime? ExpirationDate { get; set; }

        /// <summary>
        /// Ставка FTL
        /// </summary>
        public decimal? FtlRate { get; set; }
        /// <summary>
        /// Ставка LTL 1
        /// </summary>
        public decimal? LtlRate1 { get; set; }
        /// <summary>
        /// Ставка LTL 2
        /// </summary>
        public decimal? LtlRate2 { get; set; }
        /// <summary>
        /// Ставка LTL 3
        /// </summary>
        public decimal? LtlRate3 { get; set; }
        /// <summary>
        /// Ставка LTL 4
        /// </summary>
        public decimal? LtlRate4 { get; set; }
        /// <summary>
        /// Ставка LTL 5
        /// </summary>
        public decimal? LtlRate5 { get; set; }
        /// <summary>
        /// Ставка LTL 6
        /// </summary>
        public decimal? LtlRate6 { get; set; }
        /// <summary>
        /// Ставка LTL 7
        /// </summary>
        public decimal? LtlRate7 { get; set; }
        /// <summary>
        /// Ставка LTL 8
        /// </summary>
        public decimal? LtlRate8 { get; set; }
        /// <summary>
        /// Ставка LTL 9
        /// </summary>
        public decimal? LtlRate9 { get; set; }
        /// <summary>
        /// Ставка LTL 10
        /// </summary>
        public decimal? LtlRate10 { get; set; }
        /// <summary>
        /// Ставка LTL 11
        /// </summary>
        public decimal? LtlRate11 { get; set; }
        /// <summary>
        /// Ставка LTL 12
        /// </summary>
        public decimal? LtlRate12 { get; set; }
        /// <summary>
        /// Ставка LTL 13
        /// </summary>
        public decimal? LtlRate13 { get; set; }
        /// <summary>
        /// Ставка LTL 14
        /// </summary>
        public decimal? LtlRate14 { get; set; }
        /// <summary>
        /// Ставка LTL 15
        /// </summary>
        public decimal? LtlRate15 { get; set; }
        /// <summary>
        /// Ставка LTL 16
        /// </summary>
        public decimal? LtlRate16 { get; set; }
        /// <summary>
        /// Ставка LTL 17
        /// </summary>
        public decimal? LtlRate17 { get; set; }
        /// <summary>
        /// Ставка LTL 18
        /// </summary>
        public decimal? LtlRate18 { get; set; }
        /// <summary>
        /// Ставка LTL 19
        /// </summary>
        public decimal? LtlRate19 { get; set; }
        /// <summary>
        /// Ставка LTL 20
        /// </summary>
        public decimal? LtlRate20 { get; set; }
        /// <summary>
        /// Ставка LTL 21
        /// </summary>
        public decimal? LtlRate21 { get; set; }
        /// <summary>
        /// Ставка LTL 22
        /// </summary>
        public decimal? LtlRate22 { get; set; }
        /// <summary>
        /// Ставка LTL 23
        /// </summary>
        public decimal? LtlRate23 { get; set; }
        /// <summary>
        /// Ставка LTL 24
        /// </summary>
        public decimal? LtlRate24 { get; set; }
        /// <summary>
        /// Ставка LTL 25
        /// </summary>
        public decimal? LtlRate25 { get; set; }
        /// <summary>
        /// Ставка LTL 26
        /// </summary>
        public decimal? LtlRate26 { get; set; }
        /// <summary>
        /// Ставка LTL 27
        /// </summary>
        public decimal? LtlRate27 { get; set; }
        /// <summary>
        /// Ставка LTL 28
        /// </summary>
        public decimal? LtlRate28 { get; set; }
        /// <summary>
        /// Ставка LTL 29
        /// </summary>
        public decimal? LtlRate29 { get; set; }
        /// <summary>
        /// Ставка LTL 30
        /// </summary>
        public decimal? LtlRate30 { get; set; }
        /// <summary>
        /// Ставка LTL 31
        /// </summary>
        public decimal? LtlRate31 { get; set; }
        /// <summary>
        /// Ставка LTL 32
        /// </summary>
        public decimal? LtlRate32 { get; set; }
        /// <summary>
        /// Ставка LTL 33
        /// </summary>
        public decimal? LtlRate33 { get; set; }
        /// <summary>
        /// Ставка за доп. точку
        /// </summary>
        public decimal? ExtraPointRate { get; set; }
        /// <summary>
        /// Ставка за паллету Pooling
        /// </summary>
        public decimal? PoolingPalletRate { get; set; }

        /// <summary>
        /// Юр. лицо
        /// </summary>
        [ReferenceType(typeof(Company))]
        public Guid? CompanyId { get; set; }

        [SortKey(nameof(Persistables.Company.Name))]
        public Company Company { get; set; }
    }
}