using Domain.Enums;
using Domain.Extensions;
using System;

namespace Domain.Persistables
{
    /// <summary>
    /// Склад
    /// </summary>
    public class Warehouse : IPersistable, ICompanyPersistable, IMapPoint, IAddress
    {
        /// <summary>
        /// Db primary key
        /// </summary>    
        public Guid Id { get; set; }
        /// <summary>
        /// Наименование склада
        /// </summary>
        public string WarehouseName { get; set; }
        /// <summary>
        /// Клиент
        /// </summary>
        public string Client { get; set; }
        /// <summary>
        /// SoldTo number
        /// </summary>
        public string SoldToNumber { get; set; }
        /// <summary>
        /// Индекс
        /// </summary>
        public string PostalCode { get; set; }
        /// <summary>
        /// Регион
        /// </summary>
        public string Region { get; set; }
        /// <summary>
        /// Район
        /// </summary>
        public string Area { get; set; }
        /// <summary>
        /// Город
        /// </summary>
        public string City { get; set; }
        /// <summary>
        /// Улица
        /// </summary>
        public string Street { get; set; }
        /// <summary>
        /// Дом
        /// </summary>
        public string House { get; set; }
        /// <summary>
        /// Адрес
        /// </summary>
        public string Address { get; set; }
        /// <summary>
        /// Распознанный адрес
        /// </summary>
        public string ValidAddress { get; set; }
        /// <summary>
        /// Нераспознанная часть
        /// </summary>
        public string UnparsedAddressParts { get; set; }
        /// <summary>
        /// Тип комплектации
        /// </summary>
        [ReferenceType(typeof(PickingType))]
        public Guid? PickingTypeId { get; set; }

        [SortKey(nameof(Persistables.PickingType.Name))]
        public PickingType PickingType { get; set; }

        /// <summary>
        /// Leadtime, дней
        /// </summary>
        public int? LeadtimeDays { get; set; }

        /// <summary>
        /// Особенности комплектации
        /// </summary>
        public string PickingFeatures { get; set; }
        /// <summary>
        /// Способ доставки
        /// </summary>
        public DeliveryType? DeliveryType { get; set; }
        /// <summary>
        /// Активный
        /// </summary>
        public bool IsActive { get; set; }

        /// <summary>
        /// ID клиента
        /// </summary>
        public string PoolingId { get; set; }

        /// <summary>
        /// ID РЦ
        /// </summary>
        public string DistributionCenterId { get; set; }

        /// <summary>
        /// Юр. лицо
        /// </summary>
        [ReferenceType(typeof(Company))]
        public Guid? CompanyId { get; set; }

        [SortKey(nameof(Persistables.Company.Name))]
        public Company Company { get; set; }

        public decimal? Latitude { get; set; }
        public decimal? Longitude { get; set; }
        public int? GeoQuality { get; set; }

        public override string ToString()
        {
            return WarehouseName;
        }
    }
}