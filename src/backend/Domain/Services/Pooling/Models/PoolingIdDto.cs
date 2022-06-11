namespace Domain.Services.Pooling.Models
{
    public class PoolingIdDto
    {
        public string Id { get; set; }

        public string ForeignId { get; set; }

        public PoolingIdDto() { }

        public PoolingIdDto(string id, string foreignId)
        {
            if (string.IsNullOrEmpty(id))
            {
                ForeignId = foreignId;
            }
            else
            {
                Id = id;
            }
        }
    }
}
