using System.Collections.Generic;

namespace Domain.Shared.Email
{
    public class EmailMessage
    {
        public string FromEmail { get; set; }
        public string FromName { get; set; }
        public string[] ToEmails { get; set; }
        public string Subject { get; set; }
        public string Body { get; set; }
        public EmailBodyType BodyType { get; set; }
        public List<EmailAttachments> Attachments { get; set; } = new List<EmailAttachments>();
    }
}
