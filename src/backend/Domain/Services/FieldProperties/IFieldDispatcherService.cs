using System;
using System.Collections.Generic;

namespace Domain.Services.FieldProperties
{
    public interface IFieldDispatcherService
    {
        IEnumerable<FieldInfo> GetDtoFields<TDto>();
    }
}