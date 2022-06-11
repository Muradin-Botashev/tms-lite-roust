using Domain.Shared.Email;
using MailKit.Net.Smtp;
using Microsoft.Extensions.Configuration;
using MimeKit;
using Serilog;
using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace Application.Shared.Email
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;

        public EmailService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public void SendEmail(EmailMessage email)
        {
            try
            {
                string recipientsFilterPattern = _configuration.GetValue("Email:RecipientsFilter", ".*");
                Regex recipientsFilter = new Regex(recipientsFilterPattern);

                MimeMessage emailMessage = new MimeMessage();
                BodyBuilder bodyBuilder = new BodyBuilder();
                emailMessage.From.Add(new MailboxAddress(email.FromName, email.FromEmail));
                emailMessage.Subject = email.Subject;

                if (email.BodyType == EmailBodyType.Html)
                {
                    bodyBuilder.HtmlBody = email.Body;
                }
                else
                {
                    bodyBuilder.TextBody = email.Body;
                }

                foreach (string toEmail in email.ToEmails)
                {
                    bool isAllowedRecipient = recipientsFilter.IsMatch(toEmail);
                    if (isAllowedRecipient)
                    {
                        emailMessage.To.Add(new MailboxAddress(toEmail));
                    }
                    else
                    {
                        Log.Warning("Email: получатель письма {toEmail} не подходит под параметры фильтра {recipientsFilterPattern}, письмо не отправилось.", 
                                    toEmail, recipientsFilterPattern);
                    }
                }

                if (email.Attachments != null)
                {
                    foreach (var attachment in email.Attachments)
                    {
                        bodyBuilder.Attachments.Add(attachment.Name, attachment.Data);
                    }
                }

                emailMessage.Body = bodyBuilder.ToMessageBody();

                if (emailMessage.To.Count > 0)
                {
                    using (SmtpClient client = new SmtpClient())
                    {
                        var serverHost = _configuration.GetValue("Email:ServerHost", "localhost");
                        var serverPort = _configuration.GetValue("Email:ServerPort", 465);
                        var useSsl = _configuration.GetValue("Email:SslEnabled", true);
                        var userName = _configuration.GetValue("Email:UserName", "user");
                        var password = _configuration.GetValue("Email:Password", string.Empty);

                        client.Connect(serverHost, serverPort, useSsl);
                        client.Authenticate(userName, password);
                        client.Timeout = 240000;
                        client.Send(emailMessage);
                        client.Disconnect(true);
                    }

                    string recipients = string.Join(", ", emailMessage.To.Select(x => x.ToString()));
                    Log.Information("Email: письмо '{Subject}' c текстом '{Body}' было отправлено получателям: {recipients}",
                                    email.Subject, email.Body, recipients);
                }
                else
                {
                    Log.Warning("Email: пустой список получателей для письма '{Subject}' c текстом '{Body}', письмо не отправилось.",
                                email.Subject, email.Body);

                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Email: непредвиденная ошибка при отправке письма.");
            }
        }
    }
}
