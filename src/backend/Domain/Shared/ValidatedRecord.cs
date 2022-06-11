namespace Domain.Shared
{
    public class ValidatedRecord<TDto>
    {
        public TDto Data { get; set; }

        public int RecordNumber { get; set; }

        public DetailedValidationResult Result { get; set; } = new DetailedValidationResult();

        public ValidatedRecord(TDto data)
        {
            this.Data = data;
        }

        public ValidatedRecord(TDto data, int recordNumber, DetailedValidationResult result)
        {
            this.Data = data;
            this.RecordNumber = recordNumber;
            this.Result = result;
        }
    }
}
