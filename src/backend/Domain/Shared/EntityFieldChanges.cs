namespace Domain.Shared
{
    public class EntityFieldChanges
    {
        public string FieldName { get; set; }
        public object OldValue { get; set; }
        public object NewValue { get; set; }
        public bool IsManual { get; set; }
    }
}
