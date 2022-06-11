namespace Domain.Services.TaskProperties
{
    public class TaskPropertyDto : IDto
    {
        public string Id { get; set; }
        public string TaskName { get; set; }
        public string Properties { get; set; }
    }
}
