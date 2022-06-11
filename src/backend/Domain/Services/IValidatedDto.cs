using Domain.Shared;

namespace Domain.Services
{
    public interface IValidatedDto
    {
        DetailedValidationResult ValidationResult { get; set; }
    }
}
