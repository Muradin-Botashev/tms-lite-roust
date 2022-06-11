using System;

namespace Domain.Shared
{
    public class ValidationException : Exception
    {
        public ValidateResult Result { get; private set; }

        public ValidationException(ValidateResult result)
        {
            Result = result;
        }
    }
}
