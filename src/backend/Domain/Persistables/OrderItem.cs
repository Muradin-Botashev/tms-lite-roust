using System;

namespace Domain.Persistables
{
    /// <summary>
    /// Позиция заказа
    /// </summary>
    public class OrderItem : IPersistable
    {
        /// <summary>
        /// Db primary key
        /// </summary>    
        public Guid Id { get; set; }

        /// <summary>
        /// ID Заказа
        /// </summary>    
        public Guid OrderId { get; set; }

        /// <summary>
        /// Материал
        /// </summary>
        public string Nart { get; set; }

        /// <summary>
        /// Описание
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Страна происхождения
        /// </summary>
        public string CountryOfOrigin { get; set; }

        /// <summary>
        /// SPGR
        /// </summary>
        public string Spgr { get; set; }

        /// <summary>
        /// EAN
        /// </summary>
        public string Ean { get; set; }

        /// <summary>
        /// Срок годности, дней
        /// </summary>
        public int? ShelfLife { get; set; }

        /// <summary>
        /// Количество единиц товара
        /// </summary>
        public int Quantity { get; set; }

        /// <summary>
        /// Вручную изменен?
        /// </summary>
        public bool IsManualEdited { get; set; }

        public Order Order { get; set; }
    }
}
