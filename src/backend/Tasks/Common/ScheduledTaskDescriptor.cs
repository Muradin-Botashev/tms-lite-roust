using NCrontab;
using System;

namespace Tasks.Common
{
    public class ScheduledTaskDescriptor
    {
        public ScheduledTaskDescriptor(IScheduledTask task)
        {
            Task = task;
            IsActive = true;
            NextRun = DateTime.MaxValue;
        }

        public bool IsActive { get; set; }
        public IScheduledTask Task { get; set; }
        public CrontabSchedule Schedule { get; set; }
        public string Parameters { get; set; }
        public DateTime NextRun { get; set; }
    }
}
