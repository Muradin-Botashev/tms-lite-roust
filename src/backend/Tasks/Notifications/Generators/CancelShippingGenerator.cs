using Application.Shared.Notifications;
using DAL.Services;
using Domain.Enums;
using Domain.Extensions;
using Domain.Persistables;
using Domain.Shared.Email;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Tasks.Notifications.Generators
{
    public class CancelShippingGenerator : INotificationGenerator
    {
        private readonly ICommonDataService _dataService;

        public CancelShippingGenerator(ICommonDataService dataService)
        {
            _dataService = dataService;
        }

        public IEnumerable<EmailMessage> GenerateEmails(IEnumerable<NotificationEvent> events, string fromEmail, string fromName, string baseSiteUrl)
        {
            var result = new List<EmailMessage>();

            var initiators = new Dictionary<Guid, HashSet<Guid>>();
            var dataDict = new Dictionary<Guid, CancelNotificationDto>();
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
                    var data = JsonConvert.DeserializeObject<CancelNotificationDto>(@event.Data);
                    if (data != null && data.DeliveryPoints != null)
                    {
                        dataDict[@event.EntityId] = data;
                    }
                }
            }

            var entityIds = events.Select(x => x.EntityId).Distinct().ToList();
            var shippings = _dataService.GetNoTrackingDbSet<Shipping>()
                                        .Where(x => entityIds.Contains(x.Id))
                                        .ToList();

            var recipients = _dataService.GetNoTrackingDbSet<User>()
                                         .Where(x => x.IsActive 
                                                    && x.Email != null
                                                    && x.Email.Length > 0
                                                    && x.Notifications != null 
                                                    && x.Notifications.Contains((int)NotificationType.CancelShipping))
                                         .ToList();

            foreach (var shipping in shippings)
            {

                string route = string.Empty;
                if (dataDict.TryGetValue(shipping.Id, out CancelNotificationDto data))
                {
                    var routePoints = data.DeliveryPoints
                                          .OrderBy(x => x.DeliveryDate)
                                          .Select(x => x.DeliveryWarehouseName ?? x.DeliveryCity ?? x.DeliveryRegion ?? x.DeliveryAddress)
                                          .Distinct();
                    route = string.Join(" - ", routePoints);
                }

                var subject = $"Отменена перевозка {shipping.ShippingNumber} на {route}";
                var body = $@"
<html>
  <body>
    <p>Здравствуйте,</p>
    <p>
      Информируем вас о том, что перевозка {shipping.ShippingNumber} отменена.<br/>
      <a href=""{baseSiteUrl}grid/shippings/{shipping.Id.FormatGuid()}"" target=""_blank"">Зайдите в TMS</a>, чтобы проверить отмененные заявки по перевозке.
    </p>
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
            return type == NotificationType.CancelShipping;
        }
    }
}
