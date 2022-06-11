using Domain.Shared;

namespace Domain.Services.Users
{
    public class OpenTokenResponseDto : ValidateResult
    {
        public string Token { get; set; }

        public OpenTokenResponseDto(string token, string error, bool isError)
            : base(error, isError)
        {
            Token = token;
        }
    }
}
