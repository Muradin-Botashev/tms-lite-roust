using Domain.Extensions;
using System;

namespace Domain.Persistables
{
    /// <summary>
    /// Склад отгрузки
    /// </summary>
    public class ShippingWarehouse : IPersistable, ICompanyPersistable, IMapPoint, IAddress
    {
        /// <summary>
        /// Db primary key
        /// </summary>    
        public Guid Id { get; set; }
        /// <summary>
        /// Код
        /// </summary>
        public string Code { get; set; }
        /// <summary>
        /// Наименование склада
        /// </summary>
        public string WarehouseName { get; set; }
        /// <summary>
        /// Адрес
        /// </summary>
        public string Address { get; set; }
        /// <summary>
        /// Распознанный адрес
        /// </summary>
        public string ValidAddress { get; set; }
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
        /// Населенный пункт
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
        /// Pooling ID
        /// </summary>
        public string PoolingId { get; set; }

        /// <summary>
        /// ID региона
        /// </summary>
        public string PoolingRegionId { get; set; }

        /// <summary>
        /// ID Склада консолидации
        /// </summary>
        public string PoolingConsolidationId { get; set; }

        /// <summary>
        /// Активный
        /// </summary>
        public bool IsActive { get; set; }

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
