using Serilog.Core;
using Serilog.Events;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;

namespace AdvancedSerilogConfigurationWebAPI.Logger
{
    public class ExecutionContextEnricher:ILogEventEnricher
    {
        private readonly bool _includeFullContext;
        private readonly int _callerSkipFrames;

        public ExecutionContextEnricher(
            bool includeFullContext = true,
            int callerSkipFrames = 4)
        {
            _includeFullContext = includeFullContext;
            _callerSkipFrames = callerSkipFrames;
        }

        public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
        {
            var (className, methodName, filePath, lineNumber) = GetCallerContext(_callerSkipFrames);

            logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty("ClassName", className));
            logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty("MethodName", methodName));
            logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty("SourceFile", filePath));
            logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty("LineNumber", lineNumber));
            logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty("ExecutionTimestamp", DateTime.UtcNow));

            if (_includeFullContext)
            {
                var fullContext = GetFullContext(_callerSkipFrames);
                logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty("CallStackContext", fullContext));
            }
        }

        private static (string ClassName, string MethodName, string FilePath, int LineNumber)
            GetCallerContext(int skipFrames)
        {
            var stackTrace = new StackTrace(skipFrames, true);
            var frame = stackTrace.GetFrame(0);

            if (frame == null)
                return ("Unknown", "Unknown", "Unknown", 0);

            var method = frame.GetMethod();
            return (
                method?.DeclaringType?.Name ?? "Unknown",
                method?.Name ?? "Unknown",
                frame.GetFileName() ?? "Unknown",
                frame.GetFileLineNumber()
            );
        }

        private static string GetFullContext(int skipFrames)
        {
            try
            {
                var stackTrace = new StackTrace(skipFrames, true);
                var sb = new StringBuilder();

                for (int i = 0; i < stackTrace.FrameCount; i++)
                {
                    var frame = stackTrace.GetFrame(i);
                    if (frame == null) continue;

                    var method = frame.GetMethod();
                    sb.AppendLine(
                        $"{method?.DeclaringType?.FullName}.{method?.Name} " +
                        $"(Line {frame.GetFileLineNumber()} in {frame.GetFileName()})");
                }

                return sb.ToString().Trim();
            }
            catch
            {
                return "Unable to capture full context";
            }
        }
    }
}


