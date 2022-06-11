using Domain.Enums;
using Domain.Extensions;
using System;

namespace Domain.Persistables
{
    /// <summary>
    /// Перевозка
    /// </summary>
    public class Shipping : IPersistable, IWithDocumentsPersistable, ICompanyPersistable
    {
        /// <summary>
        /// Db primary key
        /// </summary>
        [IgnoreHistory]
        public Guid Id { get; set; }

        /// <summary>
        /// Номер перевозки
        /// </summary>
        public string ShippingNumber { get; set; }

        /// <summary>
        /// Способ доставки
        /// </summary>
        public DeliveryType? DeliveryType { get; set; }

        /// <summary>
        /// Терморежим мин. °C
        /// </summary>
        public int? TemperatureMin { get; set; }

        /// <summary>
        /// Терморежим макс. °C
        /// </summary>
        public int? TemperatureMax { get; set; }

        /// <summary>
        /// Способ тарификации
        /// </summary>
        public TarifficationType? TarifficationType { get; set; }

        /// <summary>
        /// Склад отгрузки
        /// </summary>
        [ReferenceType(typeof(ShippingWarehouse))]
        public Guid? ShippingWarehouseId { get; set; }

        [SortKey(nameof(Persistables.ShippingWarehouse.WarehouseName))]
        public ShippingWarehouse ShippingWarehouse { get; set; }

        /// <summary>
        /// Адрес отгрузки
        /// </summary>
        public string ShippingAddress { get; set; }

        /// <summary>
        /// Склад доставки
        /// </summary>
        [ReferenceType(typeof(Warehouse))]
        public Guid? DeliveryWarehouseId { get; set; }

        [SortKey(nameof(Warehouse.WarehouseName))]
        public Warehouse DeliveryWarehouse { get; set; }

        /// <summary>
        /// Адрес доставки
        /// </summary>
        public string DeliveryAddress { get; set; }

        /// <summary>
        /// Транспортная компания
        /// </summary>
        [ReferenceType(typeof(TransportCompany))]
        public Guid? CarrierId { get; set; }

        [SortKey(nameof(Persistables.TransportCompany.Title))]
        public TransportCompany Carrier { get; set; }

        /// <summary>
        /// Тип ТС
        /// </summary>
        [ReferenceType(typeof(VehicleType))]
        public Guid? VehicleTypeId { get; set; }

        [SortKey(nameof(Persistables.VehicleType.Name))]
        public VehicleType VehicleType { get; set; }

        /// <summary>
        /// Тип кузова
        /// </summary>
        [ReferenceType(typeof(BodyType))]
        public Guid? BodyTypeId { get; set; }

        [SortKey(nameof(Persistables.BodyType.Name))]
        public BodyType BodyType { get; set; }

        /// <summary>
        /// Предварительное кол-во паллет
        /// </summary>
        public int? PalletsCount { get; set; }

        /// <summary>
        /// Предварительное кол-во паллет введено вручную
        /// </summary>
        [IgnoreHistory]
        public bool ManualPalletsCount { get; set; }

        /// <summary>
        /// Фактическое кол-во паллет
        /// </summary>
        public int? ActualPalletsCount { get; set; }

        /// <summary>
        /// Фактическое кол-во паллет введено вручную
        /// </summary>
        [IgnoreHistory]
        public bool ManualActualPalletsCount { get; set; }

        /// <summary>
        /// Подтвержденное кол-во паллет
        /// </summary>
        public int? ConfirmedPalletsCount { get; set; }

        /// <summary>
        /// Подтвержденное кол-во паллет введено вручную
        /// </summary>
        [IgnoreHistory]
        public bool ManualConfirmedPalletsCount { get; set; }

        /// <summary>
        /// Плановый вес, кг
        /// </summary>
        public decimal? WeightKg { get; set; }

        /// <summary>
        /// Плановый вес введен вручную
        /// </summary>
        [IgnoreHistory]
        public bool ManualWeightKg { get; set; }

        /// <summary>
        /// Фактический вес, кг
        /// </summary>
        public decimal? ActualWeightKg { get; set; }

        /// <summary>
        /// Фактический вес введен вручную
        /// </summary>
        [IgnoreHistory]
        public bool ManualActualWeightKg { get; set; }

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
        /// Номер счета за доставку
        /// </summary>
        public string DeliveryInvoiceNumber { get; set; }

        /// <summary>
        /// Комментарии (причины отклонения от графика)
        /// </summary>
        public string DeviationReasonsComments { get; set; }

        /// <summary>
        /// Общая стоимость перевозки, с НДС
        /// </summary>
        public decimal? TotalDeliveryCost { get; set; }

        /// <summary>
        /// Общая стоимость перевозки
        /// </summary>
        public decimal? TotalDeliveryCostWithoutVAT { get; set; }

        /// <summary>
        /// Фактическая стоимость, без НДС
        /// </summary>
        public decimal? ActualTotalDeliveryCostWithoutVAT { get; set; }

        /// <summary>
        /// Прочее
        /// </summary>
        public decimal? OtherCosts { get; set; }

        /// <summary>
        /// Комментарий по расходам
        /// </summary>
        public string CostsComments { get; set; }

        /// <summary>
        /// Стоимость перевозки, без НДС
        /// </summary>
        public decimal? DeliveryCostWithoutVAT { get; set; }

        /// <summary>
        /// Базовая стоимость перевозки, без НДС
        /// </summary>
        public decimal? BasicDeliveryCostWithoutVAT { get; set; }

        /// <summary>
        /// Стоимость перевозки возврата, без НДС
        /// </summary>
        public decimal? ReturnCostWithoutVAT { get; set; }

        /// <summary>
        /// Сумма по ТТН, без НДС
        /// </summary>
        public decimal? InvoiceAmountWithoutVAT { get; set; }

        /// <summary>
        /// Дополнительные расходы на доставку, без НДС
        /// </summary>
        public decimal? AdditionalCostsWithoutVAT { get; set; }

        /// <summary>
        /// Стоимость за доп.точку, без НДС
        /// </summary>
        public decimal? ExtraPointCostsWithoutVAT { get; set; }

        /// <summary>
        /// Дополнительные расходы на доставку (комментарии)
        /// </summary>
        public string AdditionalCostsComments { get; set; }

        /// <summary>
        /// Кол-во часов простоя машин
        /// </summary>
        public decimal? TrucksDowntime { get; set; }

        /// <summary>
        /// Кол-во часов простоя машин введено вручную
        /// </summary>
        [IgnoreHistory]
        public bool ManualTrucksDowntime { get; set; }

        /// <summary>
        /// Ставка за возврат
        /// </summary>
        public decimal? ReturnRate { get; set; }

        /// <summary>
        /// Ставка за дополнительную точку
        /// </summary>
        public decimal? AdditionalPointRate { get; set; }

        /// <summary>
        /// Ставка за простой
        /// </summary>
        public decimal? DowntimeRate { get; set; }

        /// <summary>
        /// Ставка за холостую подачу
        /// </summary>
        public decimal? BlankArrivalRate { get; set; }

        /// <summary>
        /// Холостая подача
        /// </summary>
        public bool? BlankArrival { get; set; }

        /// <summary>
        /// Транспортная накладная
        /// </summary>
        public bool? Waybill { get; set; }

        /// <summary>
        /// Товарная накладная(Торг-12)
        /// </summary>
        public bool? WaybillTorg12 { get; set; }

        /// <summary>
        /// Товарно-Транспортная накладная +Транспортный раздел
        /// </summary>
        public bool? TransportWaybill { get; set; }

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
        /// Номер счет-фактуры
        /// </summary>
        public string InvoiceNumber { get; set; }

        /// <summary>
        /// Статус
        /// </summary>
        [IgnoreHistory]
        public ShippingState? Status { get; set; }

        /// <summary>
        /// Расходы подтверждена грузоотправителем
        /// </summary>
        public bool? CostsConfirmedByShipper { get; set; }

        /// <summary>
        /// Расходы подтверждена ТК
        /// </summary>
        public bool? CostsConfirmedByCarrier { get; set; }

        /// <summary>
        /// Дата создания перевозки
        /// </summary>
        [IgnoreHistory]
        public DateTime? ShippingCreationDate { get; set; }

        [IgnoreHistory]
        public bool ManualTarifficationType { get; set; }

        /// <summary>
        /// Пулинг набран
        /// </summary>
        public bool? IsPooling { get; set; }

        /// <summary>
        /// ФИО водителя
        /// </summary>
        public string DriverName { get; set; }

        /// <summary>
        /// Телефон водителя
        /// </summary>
        public string DriverPhone { get; set; }

        /// <summary>
        /// Паспортные данные водителя
        /// </summary>
        public string DriverPassportData { get; set; }

        /// <summary>
        /// Номер ТС
        /// </summary>
        public string VehicleNumber { get; set; }

        /// <summary>
        /// Марка ТС
        /// </summary>
        public string VehicleMake { get; set; }

        /// <summary>
        /// Номер прицепа
        /// </summary>
        public string TrailerNumber { get; set; }

        /// <summary>
        /// ID брони в Pooling
        /// </summary>
        [IgnoreHistory]
        public string PoolingReservationId { get; set; }

        /// <summary>
        /// ID слота
        /// </summary>
        [IgnoreHistory]
        public string SlotId { get; set; }

        /// <summary>
        /// Дата консолидации
        /// </summary>
        [IgnoreHistory]
        public DateTime? ConsolidationDate { get; set; }

        /// <summary>
        /// Период доступности слота
        /// </summary>
        [IgnoreHistory]
        public DateTime? AvailableUntil { get; set; }

        /// <summary>
        /// ID скалада пулинга
        /// </summary>
        [IgnoreHistory]
        public string PoolingWarehouseId { get; set; }

        /// <summary>
        /// Подсветка новой заявки на перевозку
        /// </summary>
        [IgnoreHistory]
        public bool IsNewCarrierRequest { get; set; }

        /// <summary>
        /// Синхронизировано с пулингом
        /// </summary>
        [IgnoreHistory]
        public bool SyncedWithPooling { get; set; }

        /// <summary>
        /// Статус обновлен
        /// </summary>
        public DateTime? StatusChangedAt { get; set; }

        /// <summary>
        /// Простой на погрузке
        /// </summary>
        public decimal? LoadingDowntimeCost { get; set; }

        /// <summary>
        /// Простой на выгрузке
        /// </summary>
        public decimal? UnloadingDowntimeCost { get; set; }

        /// <summary>
        /// Дата и время отгрузки
        /// </summary>
        public DateTime? ShippingDate { get; set; }

        /// <summary>
        /// Дата и время доставки
        /// </summary>
        public DateTime? DeliveryDate { get; set; }

        /// <summary>
        /// Тип груза
        /// </summary>
        public PoolingProductType? PoolingProductType { get; set; }

        /// <summary>
        /// Юр. лицо
        /// </summary>
        [IgnoreHistory]
        [ReferenceType(typeof(Company))]
        public Guid? CompanyId { get; set; }

        [SortKey(nameof(Persistables.Company.Name))]
        public Company Company { get; set; }

        /// <summary>
        /// Номер маршрута
        /// </summary>
        public string RouteNumber { get; set; }

        /// <summary>
        /// Количество бутылок
        /// </summary>
        public int? BottlesCount { get; set; }

        /// <summary>
        /// Объем 9Л
        /// </summary>
        public decimal? Volume9l { get; set; }

        /*end of fields*/

        public override string ToString()
        {
            return ShippingNumber;
        }
    }
}