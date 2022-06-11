namespace Domain.Services.Autogrouping
{
    public class RunResponse
    {
        public string RunId { get; set; }

        public string Error { get; set; }
        public bool IsError => !string.IsNullOrEmpty(Error);
    }
}
