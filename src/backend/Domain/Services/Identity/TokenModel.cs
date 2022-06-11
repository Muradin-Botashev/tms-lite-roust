namespace Domain.Services.Identity
{
    public class TokenModel
    {
        public TokenModel()
        {
        }

        public TokenModel(string token)
        {
            AccessToken = token;
        }

        public string AccessToken { get; set; }

    }
}