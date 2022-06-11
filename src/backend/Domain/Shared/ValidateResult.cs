using Domain.Services;
using System;

namespace Domain.Shared
{
    public class ValidateResult : AppResult
    {
        public ValidateResult()
        {
        }

        public ValidateResult(string error, bool isError = false)
        {
            Message = error;
            IsError = isError;
        }

        public ValidateResult(string error, Guid id, bool isError = false)
        {
            Message = error;
            Id = id;
            IsError = isError;
        }

        public ValidateResult(Guid id)
        {
            Id = id;
        }

        public Guid Id { get; set; }
    }
}