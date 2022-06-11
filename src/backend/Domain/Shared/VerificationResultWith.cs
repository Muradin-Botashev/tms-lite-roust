namespace Domain.Shared
{
    public class VerificationResultWith<T>
    {
        public VerificationResult Result { get; set; }
        public T Data { get; set; }
    }
}