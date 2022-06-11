using Domain.Extensions;

namespace Domain.Enums
{
    public enum RolePermissions
    {
        // Reserved value
        None = 0,

        /// <summary>
        /// Просмотр заказов
        /// </summary>
        [OrderNumber(0)]
        OrdersView = 1,

        /// <summary>
        /// Создание заказов
        /// </summary>
        [OrderNumber(1)]
        OrdersCreate = 2,

        /// <summary>
        /// Просмотр и прикрепление документов к заказу
        /// </summary>
        [OrderNumber(2)]
        OrdersViewAndAttachDocument = 4,

        /// <summary>
        /// Редактирование и удаление документов из заказа
        /// </summary>
        [OrderNumber(3)]
        OrdersEditAndDeleteDocument = 5,

        /// <summary>
        /// Просмотр истории заказов
        /// </summary>
        [OrderNumber(4)]
        OrdersViewHistory = 6,

        /// <summary>
        /// Просмотр перевозок
        /// </summary>
        [OrderNumber(5)]
        ShippingsView = 7,

        /// <summary>
        /// Создание перевозок
        /// </summary>
        [OrderNumber(5)]
        ShippingsCreate = 71,

        /// <summary>
        /// Просмотр и прикрепление документов к перевозке
        /// </summary>
        [OrderNumber(6)]
        ShippingsViewAndAttachDocument = 10,

        /// <summary>
        /// Редактирование и удаление документов из перевозки
        /// </summary>
        [OrderNumber(7)]
        ShippingsEditAndDeleteDocument = 11,

        /// <summary>
        /// Просмотр истории перевозок
        /// </summary>
        [OrderNumber(8)]
        ShippingsViewHistory = 12,

        /// <summary>
        /// Просмотр тарифов
        /// </summary>
        [OrderNumber(9)]
        TariffsView = 13,

        /// <summary>
        /// Редактирование тарифов
        /// </summary>
        [OrderNumber(10)]
        TariffsEdit = 14,

        /// <summary>
        /// Редактирование складов доставки
        /// </summary>
        [OrderNumber(12)]
        WarehousesEdit = 15,

        /// <summary>
        /// Редактирование артикулов
        /// </summary>
        [OrderNumber(13)]
        ArticlesEdit = 16,

        /// <summary>
        /// Редактирование типов комплектаций
        /// </summary>
        [OrderNumber(14)]
        PickingTypesEdit = 17,

        /// <summary>
        /// Редактирование транспортных компаний
        /// </summary>
        [OrderNumber(15)]
        TransportCompaniesEdit = 18,

        /// <summary>
        /// Редактирование типов ТС
        /// </summary>
        [OrderNumber(16)]
        VehicleTypesEdit = 19,

        /// <summary>
        /// Редактирование типов документов
        /// </summary>
        [OrderNumber(17)]
        DocumentTypesEdit = 20,

        /// <summary>
        /// Редактирование ролей
        /// </summary>
        [OrderNumber(18)]
        RolesEdit = 21,

        /// <summary>
        /// Редактирование пользователей
        /// </summary>
        [OrderNumber(19)]
        UsersEdit = 22,

        /// <summary>
        /// Настройка полей
        /// </summary>
        [OrderNumber(20)]
        FieldsSettings = 23,

        /// <summary>
        /// Редактирование складов отгрузки
        /// </summary>
        [OrderNumber(11)]
        ShippingWarehousesEdit = 24,

        /// <summary>
        /// Загрузка данных по водителям и ТС
        /// </summary>
        [OrderNumber(25)]
        ImportShippingVehicleDetails = 25,

        /// <summary>
        /// Сгруппировать заказы автоматически
        /// </summary>
        [OrderNumber(26)]
        AutogroupingOrders = 26,

        /// <summary>
        /// Просмотр операционного отчёта
        /// </summary>
        [OrderNumber(27)]
        ViewOperationalReport = 27,

        /// <summary>
        /// Загрузка накладных из Excel
        /// </summary>
        [OrderNumber(28)]
        ImportOrders = 28,

        /// <summary>
        /// Редактирование ЮЛ
        /// </summary>
        [OrderNumber(30)]
        CompaniesEdit = 30,

        /// <summary>
        /// Редактирование параметров автогруппировки
        /// </summary>
        [OrderNumber(31)]
        AutogroupingSettingsEdit = 31,

        /// <summary>
        /// Просмотр реестра
        /// </summary>
        [OrderNumber(32)]
        ViewRegistryReport = 32,

        /// <summary>
        /// Глобальный справочник складов Pooling
        /// </summary>
        [OrderNumber(33)]
        PoolingWarehousesImport = 33,

        /// <summary>
        /// Загрузка данных счетов и тарифов
        /// </summary>
        [OrderNumber(34)]
        InvoiceImport = 34,

        /// <summary>
        /// Редактирование Leadtime
        /// </summary>
        [OrderNumber(36)]
        LeadtimeEdit = 36,

        /// <summary>
        /// Редактирование справочника водителя
        /// </summary>
        [OrderNumber(35)]
        EditDrivers = 35,

        /// <summary>
        /// Редактирование графика отгрузок
        /// </summary>
        [OrderNumber(38)]
        EditShippingSchedule = 38,

        /// <summary>
        /// Редактирование закрепленных направлений
        /// </summary>
        [OrderNumber(19)]
        FixedDirectionsEdit = 41
    }
}