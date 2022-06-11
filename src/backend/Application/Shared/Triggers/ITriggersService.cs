using Domain.Shared;

namespace Application.Shared.Triggers
{
    public interface ITriggersService
    {
        ValidateResult Execute(bool isManual); 
    }
}