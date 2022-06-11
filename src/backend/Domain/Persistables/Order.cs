using Domain.Enums;
using Domain.Extensions;
using Domain.Services.Autogrouping;
using System;

namespace Domain.Persistables
{
    /// <summary>
    /// Заказ
    /// </summary>
    public class Order : IWithDocumentsPersistable, IPersistable, ICompanyPersistable, IAutogroupingOrder
    {
        /// <summary>
        /// Db primary key
        /// </summary>
        [IgnoreHistory]
        public Guid Id { get; set; }

        /// <summary>
        /// Статус
        /// </summary>
        [IgnoreHistory]
        public OrderState Status { get; set; }

        /// <summary>
        /// Номер накладной BDF
        /// </summary>
        public string OrderNumber { get; set; }

        /// <summary>
        /// Номер заказ клиента
        /// </summary>
        public string ClientOrderNumber { get; set; }

        /// <summary>
        /// Дата заказа
        /// </summary>
        public DateTime? OrderDate { get; set; }

        /// <summary>
        /// Тип заказа
        /// </summary>
        public OrderType? OrderType { get; set; }

        /// <summary>
        /// Плательщик
        /// </summary>
        public string Payer { get; set; }

        /// <summary>
        /// Название клиента
        /// </summary>
        public string ClientName { get; set; }

        /// <summary>
        /// Sold-to
        /// </summary>
        public string SoldTo { get; set; }

        /// <summary>
        /// Терморежим мин. °C
        /// </summary>
        public int? TemperatureMin { get; set; }

        /// <summary>
        /// Терморежим макс. °C
        /// </summary>
        public int? TemperatureMax { get; set; }

        /// <summary>
        /// Дата отгрузки
        /// </summary>
        public DateTime? ShippingDate { get; set; }

        /// <summary>
        /// Дата отгрузки введена вручную
        /// </summary>
        [IgnoreHistory]
        public bool ManualShippingDate { get; set; }

        /// <summary>
        /// Дней в пути
        /// </summary>
        public int? TransitDays { get; set; }

        /// <summary>
        /// Дата доставки
        /// </summary>
        public DateTime? DeliveryDate { get; set; }

        /// <summary>
        /// Дата доставки введена вручную
        /// </summary>
        [IgnoreHistory]
        public bool ManualDeliveryDate { get; set; }

        /// <summary>
        /// Кол-во арт.
        /// </summary>
        public int? ArticlesCount { get; set; }

        /// <summary>
        /// Предварительное Кол-во коробок
        /// </summary>
        public decimal? BoxesCount { get; set; }

        /// <summary>
        /// Подтвержденное количество коробок
        /// </summary>
        public decimal? ConfirmedBoxesCount { get; set; }

        /// <summary>
        /// Предварительное кол-во паллет
        /// </summary>
        public decimal? PalletsCount { get; set; }

        /// <summary>
        /// Предварительное кол-во паллет введено вручную
        /// </summary>
        [IgnoreHistory]
        public bool ManualPalletsCount { get; set; }

        /// <summary>
        /// Подтвежденное кол-во паллет
        /// </summary>
        public decimal? ConfirmedPalletsCount { get; set; }

        /// <summary>
        /// Фактическое кол-во паллет
        /// </summary>
        public decimal? ActualPalletsCount { get; set; }

        /// <summary>
        /// Плановый вес, кг
        /// </summary>
        public decimal? WeightKg { get; set; }

        /// <summary>
        /// Фактический вес, кг
        /// </summary>
        public decimal? ActualWeightKg { get; set; }

        /// <summary>
        /// Объем накладной, см3
        /// </summary>
        public decimal? Volume { get; set; }

        /// <summary>
        /// Сумма заказа, без НДС
        /// </summary>
        public decimal? OrderAmountExcludingVAT { get; set; }

        /// <summary>
        /// Сумма по ТТН, без НДС
        /// </summary>
        public decimal? InvoiceAmountExcludingVAT { get; set; }

        /// <summary>
        /// Регион отгрузки
        /// </summary>
        public string ShippingRegion { get; set; }

        /// <summary>
        /// Город отгрузки
        /// </summary>
        public string ShippingCity { get; set; }

        /// <summary>
        /// Регион доставки
        /// </summary>
        public string DeliveryRegion { get; set; }

        /// <summary>
        /// Город доставки
        /// </summary>
        public string DeliveryCity { get; set; }

        /// <summary>
        /// Адрес отгрузки
        /// </summary>
        public string ShippingAddress { get; set; }

        /// <summary>
        /// Адрес доставки
        /// </summary>
        public string DeliveryAddress { get; set; }

        /// <summary>
        /// Статус отгрузки
        /// </summary>
        public VehicleState ShippingStatus { get; set; }

        /// <summary>
        /// Статус доставки
        /// </summary>
        public VehicleState DeliveryStatus { get; set; }

        /// <summary>
        /// Комментарии по заказу
        /// </summary>
        public string OrderComments { get; set; }

        /// <summary>
        /// Тип комплектации
        /// </summary>
        [ReferenceType(typeof(PickingType))]
        public Guid? PickingTypeId { get; set; }

        [SortKey(nameof(Persistables.PickingType.Name))]
        public PickingType PickingType { get; set; }

        /// <summary>
        /// Тип комплектации выбран вручную
        /// </summary>
        [IgnoreHistory]
        public bool ManualPickingTypeId { get; set; }

        /// <summary>
        /// Плановое прибытие/тайм-слот (склад БДФ)
        /// </summary>
        public string PlannedArrivalTimeSlotBDFWarehouse { get; set; }

        /// <summary>
        /// Время прибытия на загрузку  (склад БДФ)
        /// </summary>
        public DateTime? LoadingArrivalTime { get; set; }

        /// <summary>
        /// Время убытия со склада БДФ
        /// </summary>
        public DateTime? LoadingDepartureTime { get; set; }

        /// <summary>
        /// Фактическая дата прибытия к грузополучателю
        /// </summary>
        public DateTime? UnloadingArrivalTime { get; set; }

        /// <summary>
        /// Дата убытия от грузополучателя
        /// </summary>
        public DateTime? UnloadingDepartureTime { get; set; }

        /// <summary>
        /// Кол-во часов простоя машин
        /// </summary>
        public decimal? TrucksDowntime { get; set; }

        /// <summary>
        /// Информация по возвратам
        /// </summary>
        public string ReturnInformation { get; set; }

        /// <summary>
        /// № счета за перевозку возврата
        /// </summary>
        public string ReturnShippingAccountNo { get; set; }

        /// <summary>
        /// Плановый срок возврата
        /// </summary>
        public DateTime? PlannedReturnDate { get; set; }

        /// <summary>
        /// Фактический срок возврата
        /// </summary>
        public DateTime? ActualReturnDate { get; set; }

        /// <summary>
        /// Номер приемного акта Мейджор
        /// </summary>
        public string MajorAdoptionNumber { get; set; }

        /// <summary>
        /// Дата создания заказа
        /// </summary>
        public DateTime? OrderCreationDate { get; set; }

        /// <summary>
        /// Товарная накладная(Торг-12)
        /// </summary>
        public bool? WaybillTorg12 { get; set; }

        /// <summary>
        /// Счет-фактура
        /// </summary>
        public bool? Invoice { get; set; }

        /// <summary>
        /// Плановая дата возврата документов
        /// </summary>
        public DateTime? DocumentsReturnDate { get; set; }

        /// <summary>
        /// Фактическая дата возврата документов
        /// </summary>
        public DateTime? ActualDocumentsReturnDate { get; set; }

        /// <summary>
        /// Перевозка
        /// </summary>
        [IgnoreHistory]
        [ReferenceType(typeof(Shipping))]
        public Guid? ShippingId { get; set; }

        public Shipping Shipping { get; set; }

        /// <summary>
        /// Номер перевозки
        /// </summary>
        [IgnoreHistory]
        public string ShippingNumber { get; set; }

        /// <summary>
        /// Статус перевозки
        /// </summary>
        [IgnoreHistory]
        public ShippingState? OrderShippingStatus { get; set; }

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
        [IgnoreHistory]
        [ReferenceType(typeof(Warehouse))]
        public Guid? DeliveryWarehouseId { get; set; }

        [SortKey(nameof(Warehouse.WarehouseName))]
        public Warehouse DeliveryWarehouse { get; set; }

        /// <summary>
        /// Активный?
        /// </summary>
        [IgnoreHistory]
        public bool IsActive { get; set; }

        /*end of fields*/

        public override string ToString()
        {
            return OrderNumber;
        }

        /// <summary>
        /// Дата изменения
        /// </summary>
        [IgnoreHistory]
        public DateTime? OrderChangeDate { get; set; }

        /// <summary>
        /// Заказ подтвержден
        /// </summary>
        public bool OrderConfirmed { get; set; }

        /// <summary>
        /// Статус возврата документов
        /// </summary>
        public bool? DocumentReturnStatus { get; set; }

        /// <summary>
        /// Источник данных в заказе (список инжекций)
        /// </summary>
        [IgnoreHistory]
        public string Source { get; set; }

        /// <summary>
        /// Особенности комплектации
        /// </summary>
        public string PickingFeatures { get; set; }

        /// <summary>
        /// Транспортная компания
        /// </summary>
        [ReferenceType(typeof(TransportCompany))]
        public Guid? CarrierId { get; set; }

        [SortKey(nameof(Persistables.TransportCompany.Title))]
        public TransportCompany Carrier { get; set; }

        /// <summary>
        /// Способ доставки
        /// </summary>
        public DeliveryType? DeliveryType { get; set; }

        /// <summary>
        /// Комментарий (причины отклонения от графика)
        /// </summary>
        public string DeviationsComment { get; set; }

        /// <summary>
        /// Базовая стоимость, без НДС
        /// </summary>
        public decimal? DeliveryCost { get; set; }

        /// <summary>
        /// Базовая стоимость введена вручную
        /// </summary>
        [IgnoreHistory]
        public bool ManualDeliveryCost { get; set; }

        /// <summary>
        /// Фактическая стоимость, без НДС
        /// </summary>
        public decimal? ActualDeliveryCost { get; set; }

        /// <summary>
        /// Способ тарификации
        /// </summary>
        public TarifficationType? TarifficationType { get; set; }

        /// <summary>
        /// Тип ТС
        /// </summary>
        [ReferenceType(typeof(VehicleType))]
        public Guid? VehicleTypeId { get; set; }

        [SortKey(nameof(Persistables.VehicleType.Name))]
        public VehicleType VehicleType { get; set; }

        /// <summary>
        /// Новый для подтвержденного
        /// </summary>
        [IgnoreHistory]
        public bool IsNewForConfirmed { get; set; }

        /// <summary>
        /// Номер брони
        /// </summary>
        public string BookingNumber { get; set; }

        /// <summary>
        /// Сумма за простой
        /// </summary>
        public decimal? DowntimeAmount { get; set; }

        /// <summary>
        /// Прочие расходы
        /// </summary>
        public decimal? OtherExpenses { get; set; }

        /// <summary>
        /// Общая стоимость без НДС
        /// </summary>
        public decimal? TotalAmount { get; set; }

        /// <summary>
        /// Общая стоимость с НДС
        /// </summary>
        public decimal? TotalAmountNds { get; set; }

        /// <summary>
        /// Стоимость перевозки возврата
        /// </summary>
        public decimal? ReturnShippingCost { get; set; }

        /// <summary>
        /// Номер счета за доставку
        /// </summary>
        public string DeliveryAccountNumber { get; set; }

        /// <summary>
        /// Документ прикреплен
        /// </summary>
        public bool DocumentAttached { get; set; }

        /// <summary>
        /// Сумма подтверждена
        /// </summary>
        public bool? AmountConfirmed { get; set; }

        /// <summary>
        /// ФИО водителя
        /// </summary>
        public string DriverName { get; set; }

        /// <summary>
        /// Телефон водителя
        /// </summary>
        public string DriverPhone { get; set; }

        /// <summary>
        /// Номер ТС
        /// </summary>
        public string VehicleNumber { get; set; }

        /// <summary>
        /// Пулинг набран
        /// </summary>
        public bool? IsPooling { get; set; }

        /// <summary>
        /// Паспортные данные водителя
        /// </summary>
        public string DriverPassportData { get; set; }

        /// <summary>
        /// Марка ТС
        /// </summary>
        public string VehicleMake { get; set; }

        /// <summary>
        /// Номер прицепа
        /// </summary>
        public string TrailerNumber { get; set; }

        /// <summary>
        /// Статус отгрузки заказа
        /// </summary>
        public WarehouseOrderState ShippingWarehouseState { get; set; }

        /// <summary>
        /// Подсветка новой заявки на перевозку
        /// </summary>
        [IgnoreHistory]
        public bool IsNewCarrierRequest { get; set; }

        /// <summary>
        /// Тип кузова
        /// </summary>
        [ReferenceType(typeof(BodyType))]
        public Guid? BodyTypeId { get; set; }

        [SortKey(nameof(Persistables.BodyType.Name))]
        public BodyType BodyType { get; set; }

        [IgnoreHistory]
        public bool ManualBodyTypeId { get; set; }

        /// <summary>
        /// Возврат
        /// </summary>
        public bool? IsReturn { get; set; }

        /// <summary>
        /// Статус обновлен
        /// </summary>
        [IgnoreHistory]
        public DateTime? StatusChangedAt { get; set; }

        /// <summary>
        /// Юр. лицо
        /// </summary>
        [IgnoreHistory]
        [ReferenceType(typeof(Company))]
        public Guid? CompanyId { get; set; }

        [SortKey(nameof(Persistables.Company.Name))]
        public Company Company { get; set; }

        /// <summary>
        /// Транспортная зона
        /// </summary>
        public string TransportZone { get; set; }

        /// <summary>
        /// Количество бутылок
        /// </summary>
        public int? BottlesCount { get; set; }

        /// <summary>
        /// Объем 9Л
        /// </summary>
        public decimal? Volume9l { get; set; }

        /// <summary>
        /// Условия платежа
        /// </summary>
        public string PaymentCondition { get; set; }
    }
}