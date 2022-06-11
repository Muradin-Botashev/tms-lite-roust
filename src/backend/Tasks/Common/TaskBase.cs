using Domain.Services.TaskProperties;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Tasks.Statistics;

namespace Tasks.Common
{
    public abstract class TaskBase<TArgs> where TArgs : PropertiesBase
    {
        public string TaskName
        {
            get
            {
                string taskSuffix = "Task";
                string result = GetType().Name;
                if (result.EndsWith(taskSuffix))
                {
                    result = result.Substring(0, result.Length - taskSuffix.Length);
                }
                return result;
            }
        }

        public async Task Execute(IServiceProvider serviceProvider, string consoleParameters, CancellationToken cancellationToken)
        {
            var parameters = LoadParameters(serviceProvider, consoleParameters);
            foreach (var param in parameters)
            {
                string args = JsonConvert.SerializeObject(param);
                Log.Logger.Information("Выполнение задачи {TaskName} с параметрами {args}...", TaskName, args);

                StatisticsStore.StartTask(TaskName);
                try
                {
                    await Execute(serviceProvider, param, cancellationToken);
                }
                finally
                {
                    StatisticsStore.EndTask(TaskName);
                }
            }
        }

        protected abstract Task Execute(IServiceProvider serviceProvider, TArgs parameters, CancellationToken cancellationToken);

        private List<TArgs> LoadParameters(IServiceProvider serviceProvider, string consoleParameters)
        { 
            var parametersStrList = GetParameters(serviceProvider, consoleParameters);
            var parametersList = parametersStrList.Select(CreateProperties).ToList();
            return parametersList;
        }

        private List<string> GetParameters(IServiceProvider serviceProvider, string consoleParameters)
        {
            var result = new List<string>();

            // 1. Параметры из консоли
            if (!string.IsNullOrEmpty(consoleParameters))
            {
                result.Add(consoleParameters);
            }

            // 2. Параметры из базы
            if (!result.Any())
            {
                var propertiesService = serviceProvider.GetService<ITaskPropertiesService>();
                var propertiesEntries = propertiesService.GetByTaskName(TaskName);
                result.AddRange(propertiesEntries.Select(p => p.Properties));
            }

            // 3. Параметры из конфига
            if (!result.Any())
            {
                var configuration = serviceProvider.GetService<IConfiguration>();
                string[] configParameters = configuration.GetSection($"{TaskName}:Args")
                                                         .AsEnumerable()
                                                         .Select(x => x.Value)
                                                         .Where(x => !string.IsNullOrEmpty(x))
                                                         .ToArray();
                if (configParameters?.Length > 0)
                {
                    result.AddRange(configParameters);
                }
            }

            // Хотя бы пустые передаем, если нигде ничего не указано
            if (!result.Any())
            {
                result.Add(null);
            }

            return result;
        }

        private TArgs CreateProperties(string parameters)
        {
            Type propertiesType = typeof(TArgs);
            var propertiesTypeFields = propertiesType.GetProperties(BindingFlags.Public | BindingFlags.Instance);
            var isPropertiesTypeValid = propertiesTypeFields.All(pi => pi.PropertyType == typeof(string));
            if (!isPropertiesTypeValid)
                throw new NotSupportedException($"Тип параметров {propertiesType.Name} не " +
                                                "должен содержать свойств, тип которых отличается от System.String");

            var properties = (TArgs)Activator.CreateInstance(propertiesType);
            var propertiesDictionary = (parameters ?? "").Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(x => x.Split(new[] { '=' }, 2))
                .ToDictionary(k => k[0], v => v[1]);

            foreach (var propertyInfo in propertiesTypeFields)
            {
                if (propertiesDictionary.ContainsKey(propertyInfo.Name))
                    propertyInfo.SetValue(properties, propertiesDictionary[propertyInfo.Name]);
            }

            return properties;
        }
    }
}
