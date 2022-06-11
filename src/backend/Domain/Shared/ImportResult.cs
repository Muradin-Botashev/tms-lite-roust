using Domain.Services.Translations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Domain.Shared
{
    public class ImportResult
    {
        public List<DetailedValidationResult> Results { get; } = new List<DetailedValidationResult>();

        public int ErrorsCount => this.Results.Where(i => i.IsError).Count();

        public int SuccessCount => this.Results.Where(i => !i.IsError).Count();

        public int UpdatedCount => this.Results.Where(i => i.ResultType == ValidateResultType.Updated).Count();

        public int CreatedCount => this.Results.Where(i => i.ResultType == ValidateResultType.Created).Count();

        private IEnumerable<DetailedValidationResult> DetailedResults => this.Results
                        .Where(i => i.GetType() == typeof(DetailedValidationResult))
                        .Cast<DetailedValidationResult>();

        public int RequiredErrorsCount => this.DetailedResults
                    .Where(i => i.Errors.Any(e => e.ResultType == ValidationErrorType.ValueIsRequired))
                    .Count();

        public int InvalidDictionaryValueErrorsCount => this.DetailedResults
                    .Where(i => i.Errors.Any(e => e.ResultType == ValidationErrorType.InvalidDictionaryValue))
                    .Count();

        public int DuplicatedRecordErrorsCount => this.DetailedResults
                    .Where(i => i.Errors.Any(e => e.ResultType == ValidationErrorType.DuplicatedRecord))
                    .Count();

        public int InvalidValueFormatErrorsCount => this.DetailedResults
                    .Where(i => i.Errors.Any(e => e.ResultType == ValidationErrorType.InvalidValueFormat))
                    .Count();

        public bool IsError => Results.Any(i => i.IsError);
    }
}
