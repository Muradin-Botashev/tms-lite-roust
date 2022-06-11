namespace Domain.Shared
{
    public class FilterFormDto<TFilter> : PagingFormDto
    {
        public TFilter Filter { get; set; }
    }
}