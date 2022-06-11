using System;
using Serilog;
using Serilog.Configuration;
using Serilog.Events;
using Serilog.Formatting.Display;

namespace Infrastructure.Logging.SerilogSlackSink
{
    /// <summary>
    /// Provides extension methods on <see cref="LoggerSinkConfiguration"/>.
    /// </summary>
    public static class SlackLoggerConfigurationExtensions
    {
        public static LoggerConfiguration Slack(
            this LoggerSinkConfiguration loggerSinkConfiguration,
            string webHookUrl,
            string channel,
            LogEventLevel minimumLogEventLevel)
        {
            SlackSinkOptions slackSinkOptions = new SlackSinkOptions
            {
                WebHookUrl = webHookUrl,
                CustomChannel = channel,
                CustomUserName = "Serilog",
                MinimumLogEventLevel = LogEventLevel.Error
            };
            return loggerSinkConfiguration.Slack(slackSinkOptions);
        }

        private const string DefaultOutputTemplate = "{Message}";
        
        /// <summary>
        /// <see cref="LoggerSinkConfiguration"/> extension that provides configuration chaining.
        /// <example>
        ///     new LoggerConfiguration()
        ///         .MinimumLevel.Verbose()
        ///         .WriteTo.Slack("webHookUrl", formatter, "channel" ,"username", "icon")
        ///         .CreateLogger();
        /// </example>
        /// </summary>
        /// <param name="loggerSinkConfiguration">Instance of <see cref="LoggerSinkConfiguration"/> object.</param>
        /// <param name="slackSinkOptions">The slack sink options object.</param>
        /// <param name="formatter">A formatter, such as <see cref="MessageTemplateTextFormatter"/>, to convert the log events into
        /// text for Slack. If control of regular text formatting is required, use the other
        /// overload of <see cref="SlackSink"/> and specify the outputTemplate parameter instead.
        /// </param>
        /// <param name="restrictedToMinimumLevel"><see cref="LogEventLevel"/> value that specifies minimum logging level that will be allowed to be logged.</param>
        /// <returns>Instance of <see cref="LoggerConfiguration"/> object.</returns>
        private static LoggerConfiguration Slack(
            this LoggerSinkConfiguration loggerSinkConfiguration,
            SlackSinkOptions slackSinkOptions,
            IFormatProvider formatProvider = null)
        {
            if (loggerSinkConfiguration == null)
            {
                throw new ArgumentNullException(nameof(loggerSinkConfiguration));
            }

            if (slackSinkOptions == null)
            {
                throw new ArgumentNullException(nameof(slackSinkOptions));
            }

            if (string.IsNullOrWhiteSpace(slackSinkOptions.WebHookUrl))
            {
                throw new ArgumentNullException(nameof(slackSinkOptions.WebHookUrl));
            }

            var formatter = new MessageTemplateTextFormatter(DefaultOutputTemplate, formatProvider);

            return loggerSinkConfiguration.Sink(new SlackSink(slackSinkOptions, formatter), slackSinkOptions.MinimumLogEventLevel);
        }
    }
}