namespace Domain.Shared
{
    public enum VerificationResult
    {
        Ok,
        Forbidden,
        WrongCredentials,
        Unauthorized,
        NotFound,
        InternalError
    }
}