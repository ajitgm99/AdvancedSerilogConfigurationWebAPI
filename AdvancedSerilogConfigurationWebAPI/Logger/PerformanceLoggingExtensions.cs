using Serilog;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace AdvancedSerilogConfigurationWebAPI.Logger
{
    public static class PerformanceLoggingExtensions
    {
        public static IDisposable TrackPerformance(
           this Serilog.ILogger logger,
           [CallerMemberName] string methodName = "",
           [CallerFilePath] string sourceFilePath = "",
           [CallerLineNumber] int sourceLineNumber = 0)
        {
            return new PerformanceTracker(
                logger,
                methodName,
                sourceFilePath,
                sourceLineNumber);
        }

        public static void LogExecutionTime(
            this Serilog.ILogger logger,
            Action action,
            string operationName,
            [CallerMemberName] string methodName = "",
            [CallerFilePath] string sourceFilePath = "",
            [CallerLineNumber] int sourceLineNumber = 0)
        {
            var stopwatch = Stopwatch.StartNew();
            try
            {
                action();
                stopwatch.Stop();

                logger.Information(
                    "Operation '{OperationName}' completed in {ElapsedMilliseconds}ms " +
                    "[Method: {MethodName}, File: {SourceFile}, Line: {LineNumber}]",
                    operationName,
                    stopwatch.ElapsedMilliseconds,
                    methodName,
                    sourceFilePath,
                    sourceLineNumber);
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                logger.Error(
                    ex,
                    "Operation '{OperationName}' failed after {ElapsedMilliseconds}ms " +
                    "[Method: {MethodName}, File: {SourceFile}, Line: {LineNumber}]",
                    operationName,
                    stopwatch.ElapsedMilliseconds,
                    methodName,
                    sourceFilePath,
                    sourceLineNumber);
                throw;
            }
        }

        public static async Task LogExecutionTimeAsync(
            this Serilog.ILogger logger,
            Func<Task> asyncAction,
            string operationName,
            [CallerMemberName] string methodName = "",
            [CallerFilePath] string sourceFilePath = "",
            [CallerLineNumber] int sourceLineNumber = 0)
        {
            var stopwatch = Stopwatch.StartNew();
            try
            {
                await asyncAction();
                stopwatch.Stop();

                logger.Information(
                    "Async operation '{OperationName}' completed in {ElapsedMilliseconds}ms " +
                    "[Method: {MethodName}, File: {SourceFile}, Line: {LineNumber}]",
                    operationName,
                    stopwatch.ElapsedMilliseconds,
                    methodName,
                    sourceFilePath,
                    sourceLineNumber);
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                logger.Error(
                    ex,
                    "Async operation '{OperationName}' failed after {ElapsedMilliseconds}ms " +
                    "[Method: {MethodName}, File: {SourceFile}, Line: {LineNumber}]",
                    operationName,
                    stopwatch.ElapsedMilliseconds,
                    methodName,
                    sourceFilePath,
                    sourceLineNumber);
                throw;
            }
        }

        private class PerformanceTracker : IDisposable
        {
            private readonly Serilog.ILogger _logger;
            private readonly string _methodName;
            private readonly string _sourceFilePath;
            private readonly int _sourceLineNumber;
            private readonly Stopwatch _stopwatch;
            private bool _disposed;

            public PerformanceTracker(
                Serilog.ILogger logger,
                string methodName,
                string sourceFilePath,
                int sourceLineNumber)
            {
                _logger = logger;
                _methodName = methodName;
                _sourceFilePath = sourceFilePath;
                _sourceLineNumber = sourceLineNumber;
                _stopwatch = Stopwatch.StartNew();

                _logger.Debug(
                    "Starting execution tracking for method {MethodName} " +
                    "[File: {SourceFile}, Line: {LineNumber}]",
                    _methodName,
                    _sourceFilePath,
                    _sourceLineNumber);
            }

            public void Dispose()
            {
                if (_disposed) return;

                _stopwatch.Stop();
                _logger.Information(
                    "Method {MethodName} executed in {ElapsedMilliseconds}ms " +
                    "[File: {SourceFile}, Line: {LineNumber}]",
                    _methodName,
                    _stopwatch.ElapsedMilliseconds,
                    _sourceFilePath,
                    _sourceLineNumber);

                _disposed = true;
            }
        }
    }
}
