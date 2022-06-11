using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using Tasks.Common;
using Tasks.Web.Models;

namespace Tasks.Statistics
{
    public static class StatisticsStore
    {
        private static readonly object _lock = new object();

        private static readonly Dictionary<string, int> _filesQueueLength = new Dictionary<string, int>();
        private static readonly Dictionary<string, int> _entriesQueueLength = new Dictionary<string, int>();
        private static readonly Dictionary<string, ScheduledTaskDescriptor> _tasks = new Dictionary<string, ScheduledTaskDescriptor>();
        private static readonly Dictionary<string, DateTime?> _lastStartTime = new Dictionary<string, DateTime?>();
        private static readonly Dictionary<string, DateTime?> _lastRunningTime = new Dictionary<string, DateTime?>();

        public static void InitializeTask(ScheduledTaskDescriptor task)
        {
            lock (_lock)
            {
                var taskName = task.Task.TaskName;
                _tasks[taskName] = task;
                _lastStartTime[taskName] = null;
                _lastRunningTime[taskName] = null;
            }
        }

        public static void StartTask(string taskName)
        {
            if (!string.IsNullOrEmpty(taskName))
            {
                lock (_lock)
                {
                    _lastStartTime[taskName] = DateTime.UtcNow;
                    _lastRunningTime[taskName] = DateTime.UtcNow;
                }
            }
        }

        public static void UpdateFilesQueueLength(string taskName, int count)
        {
            if (!string.IsNullOrEmpty(taskName))
            {
                lock (_lock)
                {
                    _filesQueueLength[taskName] = count;
                }
            }
        }

        public static void UpdateEntriesQueueLength(string taskName, int count)
        {
            if (!string.IsNullOrEmpty(taskName))
            {
                lock (_lock)
                {
                    _entriesQueueLength[taskName] = count;
                }
            }
        }

        public static void EndTask(string taskName)
        {
            if (!string.IsNullOrEmpty(taskName))
            {
                lock (_lock)
                {
                    _lastRunningTime[taskName] = DateTime.UtcNow;

                    if (_filesQueueLength.ContainsKey(taskName))
                    {
                        _filesQueueLength.Remove(taskName);
                    }

                    if (_entriesQueueLength.ContainsKey(taskName))
                    {
                        _entriesQueueLength.Remove(taskName);
                    }

                    _lastStartTime[taskName] = null;
                }
            }
        }

        public static string GetCurrentData()
        {
            StringBuilder result = new StringBuilder();

            lock (_lock)
            {
                result.AppendLine($@"tms_lite_tasks_running_tasks {_lastStartTime.Values.Where(x => x != null).Count()}");

                foreach (var entity in _filesQueueLength)
                {
                    result.AppendLine($@"tms_lite_tasks_queue_files{{task = ""{entity.Key}""}} {entity.Value}");
                }

                foreach (var entity in _entriesQueueLength)
                {
                    result.AppendLine($@"tms_lite_tasks_queue_entries{{task = ""{entity.Key}""}} {entity.Value}");
                }

                foreach (var entity in _lastStartTime)
                {
                    var value = 0.0;
                    if (entity.Value != null)
                    {
                        value = (DateTime.UtcNow - entity.Value.Value).TotalMinutes;
                    }
                    result.AppendLine($@"tms_lite_tasks_running_duration{{task = ""{entity.Key}""}} {value.ToString("0.00", CultureInfo.InvariantCulture)}");
                }

                foreach (var entity in _lastRunningTime)
                {
                    var value = entity.Value == null ? 0.0 : (DateTime.UtcNow - entity.Value.Value).TotalMinutes;
                    if (_lastStartTime.ContainsKey(entity.Key))
                    {
                        value = 0.0;
                    }
                    result.AppendLine($@"tms_lite_tasks_pause_duration{{task = ""{entity.Key}""}} {value.ToString("0.00", CultureInfo.InvariantCulture)}");
                }
            }

            return result.ToString();
        }

        public static TasksList GetWebInfo()
        {
            var tasks = new List<TaskInfo>();
            foreach (var task in _lastStartTime.OrderBy(x => x.Key))
            {
                var taskDesc = _tasks[task.Key];
                string nextTime = null;
                if (taskDesc?.Schedule != null)
                {
                    var nextRun = taskDesc.Schedule.GetNextOccurrence(DateTime.Now);
                    nextTime = GetNextTimeText(nextRun - DateTime.Now);
                }

                TaskState state;
                string time = null;
                if (task.Value == null)
                {
                    state = TaskState.Pause;
                    var lastRun = _lastRunningTime[task.Key];
                    if (lastRun != null)
                    {
                        time = GetLastTimeText(DateTime.UtcNow - lastRun.Value);
                    }
                }
                else
                {
                    state = TaskState.Running;
                    time = GetLastTimeText(DateTime.UtcNow - task.Value.Value);
                }

                tasks.Add(new TaskInfo
                {
                    Name = task.Key,
                    State = state,
                    Time = time,
                    NextTime = nextTime
                });
            }
            return new TasksList
            {
                Tasks = tasks
            };
        }

        private static string GetLastTimeText(TimeSpan time)
        {
            var totalSeconds = time.TotalSeconds;
            if (totalSeconds > 3600)
            {
                return $"завершен {(int)(totalSeconds / 3600)} ч назад";
            }
            else if (totalSeconds > 60)
            {
                return $"завершен {(int)(totalSeconds / 60)} мин назад";
            }
            else
            {
                return $"завершен {(int)totalSeconds} сек назад";
            }
        }

        private static string GetNextTimeText(TimeSpan time)
        {
            var totalSeconds = time.TotalSeconds;
            if (totalSeconds > 3600)
            {
                return $"запуск через {(int)(totalSeconds / 3600)} ч";
            }
            else if (totalSeconds > 60)
            {
                return $"запуск через {(int)(totalSeconds / 60)} мин";
            }
            else
            {
                return $"запуск через {(int)totalSeconds} сек";
            }
        }
    }
}
