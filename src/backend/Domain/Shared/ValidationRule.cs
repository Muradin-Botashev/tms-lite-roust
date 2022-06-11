using System;

namespace Domain.Shared
{
    public class ValidationRule<TDto, TEntity>
    {
        public string Field { get; set; }

        public Func<TDto, TEntity, DetailedValidationResult> Rule { get; set; }
    }
}
