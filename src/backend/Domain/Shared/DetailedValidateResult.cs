using Domain.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Domain.Shared
{
    public class DetailedValidationResult: ValidateResult
    {
        public ValidateResultType ResultType { get; set; }

        public DetailedValidationResult()
        {
            ResultType = ValidateResultType.Skipped;
        }

        public DetailedValidationResult(string error, Guid id): base(error, id, true) 
        {
            ResultType = ValidateResultType.Error;
        }

        public DetailedValidationResult(Guid id) : base(id)
        {
            ResultType = ValidateResultType.Skipped;
        }

        public DetailedValidationResult(string name, string message, ValidationErrorType errorType) 
        {
            this.AddError(name, message, errorType);
        }

        public List<ValidationResultItem> _errors = new List<ValidationResultItem>();

        public IReadOnlyCollection<ValidationResultItem> Errors => _errors.AsReadOnly();

        private string _error;

        public override string Message {
            get 
            {
                return !string.IsNullOrEmpty(_error) ? _error :
                    string.Join(", ", this.Errors.Select(i => i.Message));
            }
            set 
            {
                _error = value;
            }
        }
        
        public override bool IsError
        {
            get
            {
                return Errors.Any();
            }
        }

        public void AddError(string name, string message, ValidationErrorType errorType)
        {
            this.AddError(new ValidationResultItem
            { 
                Name = name?.ToLowerFirstLetter(),
                Message = message,
                ResultType = errorType
            });
        }

        public void AddError(ValidationResultItem error)
        {
            this.ResultType = ValidateResultType.Error;
            this._errors.Add(error);
        }

        public void AddErrors(IEnumerable<ValidationResultItem> errors)
        {
            if (errors.Count() > 0)
            {
                this.ResultType = ValidateResultType.Error;
                this._errors.AddRange(errors);
            }
        }
    }
    public enum ValidateResultType
    {
        Updated,
        Created,
        Error,
        Skipped
    }
}