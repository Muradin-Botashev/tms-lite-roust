using System;
using System.Collections.Generic;
using System.Text;

namespace Domain.Shared
{
    public class ValidationResultItem
    {
        public string Name { get; set; }

        public string Message {get; set; }

        public ValidationErrorType ResultType { get; set; }
    }

    public enum ValidationErrorType
    { 
        InvalidDictionaryValue,

        InvalidValueFormat,

        ValueIsRequired,

        DuplicatedRecord,

        Exception,

        InvalidDateRange,

        InvalidPassword,

        StringTooLong,

        ValueIsReadonly
    }
}
