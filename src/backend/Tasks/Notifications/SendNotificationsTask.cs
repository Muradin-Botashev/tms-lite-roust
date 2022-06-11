using DAL.Services;
using Domain.Persistables;
using Domain.Shared.Email;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Tasks.Common;
using Tasks.Notifications.Generators;

namespace Tasks.Notifications
{
    public class SendNotificationsTask : TaskBase<PropertiesBase>, IScheduledTask
    {
        public string Schedule => "*/5 * * * *";

        private readonly IEmailService _emailService;
        private readonly IConfiguration _configuration;
        private readonly IEnumerable<INotificationGenerator> _generators;

        public SendNotificationsTask(IEmailService emailService, IConfiguration configuration, IEnumerable<INotificationGenerator> generators)
        {
            _emailService = emailService;
            _configuration = configuration;
            _generators = generators;
        }

        protected override Task Execute(IServiceProvider serviceProvider, PropertiesBase parameters, CancellationToken cancellationToken)
        {
            var baseSiteUrl = _configuration.GetValue("SiteUrl", string.Empty);
            var fromEmail = _configuration.GetValue("Email:ReplyEmail", string.Empty);
            var fromName = _configuration.GetValue("Email:ReplyName", string.Empty);

            var dataService = serviceProvider.GetService<ICommonDataService>();
            var events = dataService.GetDbSet<NotificationEvent>()
                                    .Where(x => !x.IsProcessed)
                                    .ToList();

            foreach (var eventGroup in events.GroupBy(x => x.Type))
            {
                try
                {
                    var generator = _generators.FirstOrDefault(x => x.IsApplicable(eventGroup.Key));
                    if (generator != null)
                    {
                        var emailMessages = generator.GenerateEmails(eventGroup, fromEmail, fromName, baseSiteUrl);
                        foreach (var emailMessage in emailMessages)
                        {
                            _emailService.SendEmail(emailMessage);
                        }
                    }
                    else
                    {
                        var typeName = eventGroup.Key.ToString();
                        Log.Warning("Не найдено генератора для события {typeName}.", typeName);
                    }
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "Ошибка при подготовке событий к отправке");
                }
            }

            foreach (var @event in events)
            {
                @event.IsProcessed = true;
            }

            dataService.SaveChanges();

            return Task.CompletedTask;
        }
    }
}
