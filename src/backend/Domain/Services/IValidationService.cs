using Domain.Shared;
using System;
using System.Collections.Generic;
using System.Text;

namespace Domain.Services
{
    public interface IValidationService
    {
        DetailedValidationResult Validate<TDto>(TDto dto);

        DetailedValidationResult ValidateFiled<TDto>(TDto dto, string fieldName);
    }
}
