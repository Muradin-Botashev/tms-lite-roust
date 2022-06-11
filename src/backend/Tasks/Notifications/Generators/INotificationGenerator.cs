using Domain.Enums;
using Domain.Persistables;
using Domain.Shared.Email;
using System.Collections.Generic;

namespace Tasks.Notifications.Generators
{
    public interface INotificationGenerator
    {
        IEnumerable<EmailMessage> GenerateEmails(IEnumerable<NotificationEvent> events, string fromEmail, string fromName, string baseSiteUrl);
        bool IsApplicable(NotificationType type);
    }
}
