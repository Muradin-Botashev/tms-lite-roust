using Domain.Shared;
using System.Collections.Generic;

namespace Domain.Services
{
    public interface IStateService
    {
        IEnumerable<StateDto> GetAll<TEnum>();
        IEnumerable<LookUpDto> ForSelect<TEnum>();
    }
}
