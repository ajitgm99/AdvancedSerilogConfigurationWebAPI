using Serilog;
using Serilog.Events;
using Serilog.Exceptions;
using Serilog.Formatting.Compact;
using Serilog.Sinks.SystemConsole.Themes;


namespace AdvancedSerilogConfigurationWebAPI.Logger
{
    public static class LoggingConfiguration
    {
        public static LoggerConfiguration ConfigureExecutionLogger(
            this LoggerConfiguration loggerConfig,
            string appName,
            string environmentName)
        {
            // Create an instance of your enricher first
            var executionContextEnricher = new ExecutionContextEnricher();

            return loggerConfig
                .MinimumLevel.Debug()
                .Enrich.FromLogContext()
                .Enrich.With(executionContextEnricher) // Use the instance instead of generic
                .Enrich.WithExceptionDetails()
                .Enrich.WithProperty("Application", appName)
                .Enrich.WithProperty("Environment", environmentName)
                .Enrich.WithProcessId()
                .Enrich.WithThreadId()
                .WriteTo.Console(
                    outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] " +
                                   "{Message:lj} " +
                                   "[{ClassName}.{MethodName} @ {SourceFile}:{LineNumber}]{NewLine}{Exception}");
                //.WriteTo.File(
                //    new CompactJsonFormatter(),
                //    "logs/execution-log.json",
                //    rollingInterval: RollingInterval.Day,
                //    retainedFileCountLimit: 7);
        }
    }
}
