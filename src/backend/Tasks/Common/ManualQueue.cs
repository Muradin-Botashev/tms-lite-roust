using System.Collections.Generic;

namespace Tasks.Common
{
    public static class ManualQueue
    {
        private static readonly HashSet<string> _tasks = new HashSet<string>();

        public static void AddTask(string taskName)
        {
            lock (_tasks)
            {
                if (!string.IsNullOrEmpty(taskName))
                {
                    _tasks.Add(taskName);
                }
            }
        }

        public static bool TryDequeueTask(string taskName)
        {
            lock (_tasks)
            {
                if (_tasks.Contains(taskName))
                {
                    _tasks.Remove(taskName);
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }
    }
}
