using DAL.Services;
using Domain.Enums;
using Domain.Extensions;
using Domain.Persistables;
using Domain.Shared.Email;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Tasks.Notifications.Generators
{
    public class RejectShippingRequestGenerator : INotificationGenerator
    {
        private readonly ICommonDataService _dataService;

        public RejectShippingRequestGenerator(ICommonDataService dataService)
        {
            _dataService = dataService;
        }

        public IEnumerable<EmailMessage> GenerateEmails(IEnumerable<NotificationEvent> events, string fromEmail, string fromName, string baseSiteUrl)
        {
            var result = new List<EmailMessage>();

            var initiators = new Dictionary<Guid, HashSet<Guid>>();
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
            }

            var entityIds = events.Select(x => x.EntityId).Distinct().ToList();
            var shippings = _dataService.GetNoTrackingDbSet<Shipping>()
                                        .Where(x => entityIds.Contains(x.Id))
                                        .ToList();

            var shippingOrdersDict = _dataService.GetNoTrackingDbSet<Order>()
                                                 .Include(x => x.DeliveryWarehouse)
                                                 .Where(x => x.ShippingId != null && entityIds.Contains(x.ShippingId.Value))
                                                 .GroupBy(x => x.ShippingId)
                                                 .ToDictionary(x => x.Key, x => x.ToList());

            var recipients = _dataService.GetNoTrackingDbSet<User>()
                                         .Where(x => x.IsActive 
                                                    && x.Email != null
                                                    && x.Email.Length > 0
                                                    && x.Notifications != null 
                                                    && x.Notifications.Contains((int)NotificationType.RejectShippingRequest))
                                         .ToList();

            foreach (var shipping in shippings)
            {
                List<Order> orders;
                if (!shippingOrdersDict.TryGetValue(shipping.Id, out orders))
                {
                    orders = new List<Order>();
                }

                var routePoints = orders.OrderBy(x => x.DeliveryDate)
                                        .Select(x => x.DeliveryWarehouse?.WarehouseName ?? x.DeliveryCity ?? x.DeliveryRegion ?? x.DeliveryAddress)
                                        .Distinct();
                var route = string.Join(" - ", routePoints);

                var subject = $"Отменена перевозка {shipping.ShippingNumber} транспортной компанией на {route}";
                var body = $@"
<html>
  <body>
    <p>Здравствуйте,</p>
    <p>
      Информируем вас о том, что перевозка {shipping.ShippingNumber} отменена транспортной компанией.<br/>
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
            return type == NotificationType.RejectShippingRequest;
        }
    }
}
