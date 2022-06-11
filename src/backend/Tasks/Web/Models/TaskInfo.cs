namespace Tasks.Web.Models
{
    public class TaskInfo
    {
        public string Name { get; set; }
        public TaskState State { get; set; }
        public string Time { get; set; }
        public string NextTime { get; set; }
    }
}
