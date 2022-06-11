namespace Domain.Services
{
    public class AppResult
    {
        public virtual bool IsError { get; set; }

        public virtual string Message { get; set; }

        public bool ManuallyClosableMessage { get; set; }

        public string ConfirmationMessage { get; set; }

        public bool NeedConfirmation { get; set; }

        public AppResultType MessageType { get; set; } = AppResultType.SimpleNotification;
    }
}