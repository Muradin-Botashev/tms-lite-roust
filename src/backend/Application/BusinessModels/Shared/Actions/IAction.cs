using Domain.Enums;
using Domain.Services;
using Domain.Shared.UserProvider;

namespace Application.BusinessModels.Shared.Actions
{
    public interface IAction<T>
    {
        AppColor Color { get; set; }
        AppResult Run(CurrentUserDto user, T target);
        bool IsAvailable(T target);
    }
}