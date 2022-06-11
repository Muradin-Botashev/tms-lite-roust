using Application.Shared.Notifications;
using DAL.Services;
using Domain.Enums;
using Domain.Extensions;
using Domain.Persistables;
using Domain.Services.Translations;
using Domain.Shared.Email;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Tasks.Notifications.Generators
{
    public class UpdateShippingRequestDataGenerator : INotificationGenerator
    {
        private readonly ICommonDataService _dataService;

        public UpdateShippingRequestDataGenerator(ICommonDataService dataService)
        {
            _dataService = dataService;
        }

        public IEnumerable<EmailMessage> GenerateEmails(IEnumerable<NotificationEvent> events, string fromEmail, string fromName, string baseSiteUrl)
        {
            var result = new List<EmailMessage>();
            var lang = "ru";

            var initiators = new Dictionary<Guid, HashSet<Guid>>();
            var dataDict = new Dictionary<Guid, Dictionary<string, NotificationOrderChangesDto>>();
            foreach (var @event in events)
            {
                if (@event.InitiatorId != null)
                {
                    HashSet<Guid> userIds;
                    if (!initiators.TryGetValue(@event.EntityId, out userIds))
                    {
                        userIds = new HashSet<Guid>();
                        initiators[@event.EntityId] = userIds;
                    }
                    userIds.Add(@event.InitiatorId.Value);
                }

                if (!string.IsNullOrEmpty(@event.Data))
                {
                    var data = JsonConvert.DeserializeObject<NotificationShippingChangesDto>(@event.Data);
                    if (data != null && data.Orders != null)
                    {
                        Dictionary<string, NotificationOrderChangesDto> ordersDict;
                        if (!dataDict.TryGetValue(@event.EntityId, out ordersDict))
                        {
                            ordersDict = new Dictionary<string, NotificationOrderChangesDto>();
                            dataDict[@event.EntityId] = ordersDict;
                        }
                        foreach (var order in data.Orders)
                        {
                            if (ordersDict.TryGetValue(order.OrderNumber ?? string.Empty, out NotificationOrderChangesDto changes))
                            {
                                changes.ChangesFields = changes.ChangesFields.Concat(order.ChangesFields).Distinct().ToList();
                            }
                            else
                            {
                                ordersDict[order.OrderNumber ?? string.Empty] = order;
                            }
                        }
                    }
                }
            }

            var entityIds = events.Select(x => x.EntityId).Distinct().ToList();
            var shippings = _dataService.GetNoTrackingDbSet<Shipping>()
                                        .Include(x => x.BodyType)
                                        .Include(x => x.Company)
                                        .Include(x => x.VehicleType)
                                        .Include(x => x.VehicleType.Tonnage)
                                        .Include(x => x.VehicleType.BodyType)
                                        .Where(x => entityIds.Contains(x.Id))
                                        .ToList();

            var shippingOrdersDict = _dataService.GetNoTrackingDbSet<Order>()
                                                 .Include(x => x.ShippingWarehouse)
                                                 .Include(x => x.DeliveryWarehouse)
                                                 .Where(x => x.ShippingId != null && entityIds.Contains(x.ShippingId.Value))
                                                 .GroupBy(x => x.ShippingId)
                                                 .ToDictionary(x => x.Key, x => x.ToList());

            var recipients = _dataService.GetNoTrackingDbSet<User>()
                                         .Where(x => x.IsActive 
                                                    && x.Email != null
                                                    && x.Email.Length > 0
                                                    && x.Notifications != null 
                                                    && x.Notifications.Contains((int)NotificationType.UpdateShippingRequestData))
                                         .ToList();

            foreach (var shipping in shippings)
            {
                List<Order> orders;
                if (!shippingOrdersDict.TryGetValue(shipping.Id, out orders))
                {
                    orders = new List<Order>();
                }

                var tarifficationType = shipping.TarifficationType.FormatEnum()?.Translate(lang);
                var cargoType = shipping.PoolingProductType.FormatEnum()?.Translate(lang);
                var orderNumbers = string.Join(", ", orders.Select(x => x.OrderNumber).OrderBy(x => x));
                var clientOrderNumbers = string.Join(", ", orders.Select(x => x.ClientOrderNumber).Where(x => !string.IsNullOrEmpty(x)).OrderBy(x => x));
                var orderAmount = orders.Sum(x => x.OrderAmountExcludingVAT ?? 0M);
                var shippingWarehouse = orders.OrderBy(x => x.ShippingDate).Select(x => x.ShippingWarehouse).FirstOrDefault(x => x != null);
                var lastDeliveryAddress = orders.OrderByDescending(x => x.DeliveryDate).Select(x => x.DeliveryAddress).FirstOrDefault(x => x != null);
                var routePoints = orders.OrderBy(x => x.DeliveryDate)
                                        .Select(x => x.DeliveryWarehouse?.WarehouseName ?? x.DeliveryCity ?? x.DeliveryRegion ?? x.DeliveryAddress)
                                        .Distinct();
                var route = string.Join(" - ", routePoints);

                string updates = string.Empty;
                if (dataDict.TryGetValue(shipping.Id, out Dictionary<string, NotificationOrderChangesDto> data))
                {
                    updates = string.Join("", data.Values.OrderBy(x => x.OrderNumber).Select(x => GetOrderFieldUpdates(x, lang)));
                }

                var subject = $"Обновленные данные по {tarifficationType} перевозке {shipping.ShippingNumber} на {route}";
                var body = $@"
<html>
  <body>
    <p>Здравствуйте,</p>
    <p>
      Информируем вас о том, что в перевозке {shipping.ShippingNumber} изменены данные по <номер заявки> заявке на {lastDeliveryAddress}.<br/>
      <a href=""{baseSiteUrl}grid/shippings/{shipping.Id.FormatGuid()}"" target=""_blank"">Зайдите в TMS</a>, чтобы проверить обновленные данные.
    </p>
    <p>
      {updates}
    </p>
    <p>
      Номера заказов: {clientOrderNumbers}<br/>
      Номера накладных: {orderNumbers}<br/>
      Грузоотправитель: {shipping.Company?.Name}<br/>
      Стоимость перевозки: {shipping.TotalDeliveryCostWithoutVAT.FormatDecimal(2)} ₽<br/>
      Склад отгрузки: {shippingWarehouse?.WarehouseName}<br/>
      Дата и время отгрузки: {shipping.ShippingDate.FormatDateTime()}<br/>
      Направление: {route}<br/>
      Дата доставки: {shipping.DeliveryDate.FormatDate()}<br/>
      Тип транспорта: {shipping.BodyType?.Name}<br/>
      Грузоподъёмность: {shipping.VehicleType?.Tonnage?.Name}<br/>
      Тип груза: {cargoType}<br/>
      Количество паллет: {shipping.PalletsCount}<br/>
      Масса: {shipping.WeightKg.FormatDecimal(3)} кг<br/>
      Стоимость: {orderAmount.FormatDecimal(2)} ₽
    </p>
    <p>Если вы не оформляли данную заявку, напишите нам.</p>
    <p>С уважением, команда TMS Roust</p>
  </body>
</html>
                 ";

                var ignoreUserIds = initiators[shipping.Id];
                var shippingRecipients = recipients.Where(x => (x.CarrierId == null || x.CarrierId == shipping.CarrierId)
                                                            && x.CompanyId == shipping.CompanyId
                                                            && !ignoreUserIds.Contains(x.Id))
                                                   .ToList();
                foreach (var recipient in shippingRecipients)
                {
                    result.Add(new EmailMessage
                    {
                        FromEmail = fromEmail,
                        FromName = fromName,
                        ToEmails = new[] { recipient.Email },
                        Subject = subject,
                        Body = body,
                        BodyType = EmailBodyType.Html,
                        Attachments = null
                    });
                }
            }

            return result;
        }

        public bool IsApplicable(NotificationType type)
        {
            return type == NotificationType.UpdateShippingRequestData;
        }

        private string GetOrderFieldUpdates(NotificationOrderChangesDto dto, string lang)
        {
            var fieldUpdates = string.Join(", ", (dto.ChangesFields ?? new List<string>()).Select(x => x.ToLowerFirstLetter().Translate(lang)));
            if (string.IsNullOrEmpty(dto.OrderNumber))
            {
                return $"В перевозке изменены данные в поле {fieldUpdates}.<br/>";
            }
            else
            {
                return $"В заказе {dto.OrderNumber} изменены данные в поле {fieldUpdates}.<br/>";
            }
        }
    }
}
