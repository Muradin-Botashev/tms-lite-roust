using Infrastructure.Logging.SerilogSlackSink;
using Microsoft.Extensions.Configuration;
using Serilog;
using Serilog.Events;
using System;
using System.IO;

namespace Infrastructure.Logging
{
    public static class LoggerFactory
    {
        public static ILogger CreateLogger(IConfiguration configuration, string projectName)
        {
            var baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
            string messageTemplate = "{Timestamp:dd.MM.yyyy HH:mm:ss.fff zzz} ({RequestId}) [{Level}] {Message}{NewLine}{Exception}";

            LoggerConfiguration loggerConfiguration = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .Enrich.FromLogContext();

            bool isConsoleLogEnabled = configuration.GetValue("Logging:Console:Enabled", false);
            if (isConsoleLogEnabled)
            {
                loggerConfiguration = loggerConfiguration.WriteTo.Console(outputTemplate: messageTemplate);
            }

            bool isFileLogEnabled = configuration.GetValue("Logging:File:Enabled", false);
            if (isFileLogEnabled)
            {
                int asyncBufferSize = configuration.GetValue("Logging:File:AsyncBufferSize", 1000);
                int maxFileSizeMb = configuration.GetValue("Logging:File:MaxSizeMb", 100);
                int maxFileSizeBytes = maxFileSizeMb * 1048576;

                loggerConfiguration = loggerConfiguration.WriteTo.Async(
                            ac => ac.RollingFile(Path.Combine(baseDirectory, $"logs/{projectName}-{{Date}}.log"),
                                                 fileSizeLimitBytes: maxFileSizeBytes,
                                                 outputTemplate: messageTemplate),
                            asyncBufferSize);
            }

            bool isSlackLogEnabled = configuration.GetValue("Logging:Slack:Enabled", false);
            if (isSlackLogEnabled)
            {
                string webHookUrl = configuration["Logging:Slack:WebHookUrl"];
                string channel = configuration["Logging:Slack:Channel"];
                loggerConfiguration = loggerConfiguration.WriteTo.Slack(webHookUrl, channel, LogEventLevel.Error);
            }

            bool isElasticLogEnables = configuration.GetValue("Logging:Elastic:Enabled", false);
            if (isElasticLogEnables)
            {
                string host = configuration["Logging:Elastic:Host"];
                string port = configuration["Logging:Elastic:Port"];
                string login = configuration["Logging:Elastic:Login"];
                string password = configuration["Logging:Elastic:Password"];
                string envName = configuration["Logging:Elastic:Environment"];

                string credentials = string.Empty;
                if (!string.IsNullOrEmpty(login) && !string.IsNullOrEmpty(password))
                {
                    credentials = $"{login}:{password}@";
                }

                string uri = $"http://{credentials}{host}:{port}/";
                string indexFormat = $"tms-lite-roust-{envName.ToLower()}-{projectName.ToLower()}-{{0:yyyy.MM}}";

                loggerConfiguration = loggerConfiguration.WriteTo.Elasticsearch(
                    nodeUris: uri, 
                    indexFormat: indexFormat, 
                    restrictedToMinimumLevel: LogEventLevel.Information);
            }

            return loggerConfiguration.CreateLogger();
        }
    }
}
