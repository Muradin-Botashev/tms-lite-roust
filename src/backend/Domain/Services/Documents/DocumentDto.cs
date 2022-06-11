namespace Domain.Services.Documents
{
    public class DocumentDto : IDto
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string FileId { get; set; }
        public string TypeId { get; set; }
    }
}
