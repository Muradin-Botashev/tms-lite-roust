using DAL.Services;
using Domain.Persistables;
using Domain.Services.Translations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NCrontab;
using Serilog;
using Serilog.Context;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Tasks.Common;
using Tasks.Statistics;

namespace Tasks
{
    public class ScheduleWorker : BackgroundService
    {
        private bool _isDisposed = false;

        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly IConfiguration _configuration;
        private readonly List<ScheduledTaskDescriptor> _tasks;
        private readonly HashSet<string> _runningTasks;

        public ScheduleWorker(
            IServiceScopeFactory serviceScopeFactory,
            IConfiguration configuration)
        {
            _serviceScopeFactory = serviceScopeFactory;
            _configuration = configuration;

            CreateLogger();

            string env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production";
            Log.Logger.Information("Менеджер задач запущен в режиме {env}", env);

            _tasks = InitializeTasks();
            _runningTasks = new HashSet<string>();
        }

        public override void Dispose()
        {
            if (!_isDisposed)
            {
                Log.CloseAndFlush();
                _isDisposed = true;
            }
            base.Dispose();
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            InitializeTranslations();

            while (!stoppingToken.IsCancellationRequested)
            {
                foreach (var task in _tasks)
                {
                    string taskName = task.Task.TaskName;

                    if (ManualQueue.TryDequeueTask(taskName) || (task.IsActive && task.NextRun <= DateTime.Now))
                    {
                        if (_runningTasks.Contains(taskName))
                        {
                            continue;
                        }

                        ExecuteTaskThread(task, stoppingToken);
                    }
                }

                if (!_tasks.Any(t => t.IsActive))
                {
                    break;
                }

                await Task.Delay(1000, stoppingToken);
            }

            Log.Logger.Information("Завершена работа менеджера задач");
        }

        private async Task ExecuteTaskThread(ScheduledTaskDescriptor task, CancellationToken cancellationToken)
        {
            string taskName = task.Task.TaskName;
            try
            {
                _runningTasks.Add(taskName);
                Log.Logger.Information("Начало выполнения задачи {taskName}", taskName);

                await Task.Run(() => TaskThread(task, cancellationToken), cancellationToken);

                Log.Logger.Information("Задача {taskName} завершена успешно", taskName);
            }
            catch (Exception ex)
            {
                Log.Logger.Error(ex, "Ошибка при выполнении задачи {taskName}", taskName);
            }
            finally
            {
                _runningTasks.Remove(taskName);

                if (task.Schedule == null)
                {
                    task.IsActive = false;
                }
                else
                {
                    task.NextRun = task.Schedule.GetNextOccurrence(DateTime.Now);

                    string nextRun = task.NextRun.ToString("dd.MM.yyyy HH:mm:ss");
                    Log.Logger.Information("Следующий запуск задачи {taskName} запланирован на {nextRun}", taskName, nextRun);
                }
            }
        }

        private void TaskThread(ScheduledTaskDescriptor task, CancellationToken cancellationToken)
        {
            using (var scope = _serviceScopeFactory.CreateScope())
            using (LogContext.PushProperty("TaskName", task.Task.TaskName))
            {
                task.Task.Execute(scope.ServiceProvider, task.Parameters, cancellationToken).Wait();
            }
        }

        private void InitializeTranslations()
        {
            using (var scope = _serviceScopeFactory.CreateScope())
            {
                var db = scope.ServiceProvider.GetService<ICommonDataService>();
                var translations = db.GetDbSet<Translation>().ToList();
                TranslationProvider.FillCache(translations);
            }
        }

        private List<ScheduledTaskDescriptor> InitializeTasks()
        {
            var result = new List<ScheduledTaskDescriptor>();
            using (var scope = _serviceScopeFactory.CreateScope())
            {
                string runConsoleArg = _configuration.GetValue<string>("run", null);
                string argsConsoleArg = _configuration.GetValue<string>("args", null);

                var tasks = scope.ServiceProvider.GetService<IEnumerable<IScheduledTask>>();
                foreach (IScheduledTask task in tasks)
                {
                    if (!string.IsNullOrEmpty(runConsoleArg) && string.Compare(task.TaskName, runConsoleArg, true) != 0)
                    {
                        continue;
                    }

                    var taskDesc = new ScheduledTaskDescriptor(task);

                    StatisticsStore.InitializeTask(taskDesc);

                    if (string.IsNullOrEmpty(runConsoleArg))
                    {
                        string schedule = _configuration.GetValue<string>($"{task.TaskName}:Schedule", null);
                        if (string.IsNullOrEmpty(schedule))
                        {
                            schedule = task.Schedule;
                        }

                        taskDesc.Schedule = CrontabSchedule.TryParse(schedule);
                        if (taskDesc.Schedule == null)
                        {
                            continue;
                        }
                        taskDesc.NextRun = taskDesc.Schedule.GetNextOccurrence(DateTime.Now);

                        string firstRun = taskDesc.NextRun.ToString("dd.MM.yyyy HH:mm:ss");
                        Log.Logger.Information("Задача {TaskName} добавлена в план, первый запуск назначен на {firstRun}", task.TaskName, firstRun);
                    }
                    else
                    {
                        taskDesc.Parameters = argsConsoleArg;
                        taskDesc.NextRun = DateTime.Now;
                    }

                    result.Add(taskDesc);
                }
            }
            return result;
        }

        private void CreateLogger()
        {
            Log.Logger = Infrastructure.Logging.LoggerFactory.CreateLogger(_configuration, "Tasks");
        }
    }
}
