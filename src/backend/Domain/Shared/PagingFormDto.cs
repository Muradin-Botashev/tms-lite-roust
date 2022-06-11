namespace Domain.Shared
{
    public class PagingFormDto
    {
        public int Skip { get; set; }
        public int Take { get; set; }

        public SortingDto Sort { get; set; }
    }
}