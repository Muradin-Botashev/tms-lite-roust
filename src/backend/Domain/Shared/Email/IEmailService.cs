namespace Domain.Shared.Email
{
    public interface IEmailService
    {
        void SendEmail(EmailMessage email);
    }
}
