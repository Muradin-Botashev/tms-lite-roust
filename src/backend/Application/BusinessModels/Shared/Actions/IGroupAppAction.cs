using System.Collections.Generic;

namespace Application.BusinessModels.Shared.Actions
{
    public interface IGroupAppAction<T> : IAction<IEnumerable<T>>
    {
        /// <summary>
        /// Is action allowed for single record
        /// </summary>
        bool IsSingleAllowed { get; }
    }
}