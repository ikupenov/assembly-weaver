using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using MonoCecilWeaver.Target;

namespace MonoCecilWeaver.Handlers
{
    public class PerformanceLogger : PerformanceProfiler
    {
        private readonly string profilerLogPath = ConfigurationManager.AppSettings["ProfilerLogPath"];

        private readonly Stopwatch stopwatch;
        private readonly ILogger fileLogger;

        public PerformanceLogger(MethodBase method) : base(method)
        {
            this.stopwatch = new Stopwatch();
            this.fileLogger = LoggerFactory.CreateLogger(this.profilerLogPath);
        }

        public override void Start()
        {
            this.stopwatch.Reset();
            this.stopwatch.Start();
        }

        public override void Stop()
        {
            this.stopwatch.Stop();

            var methodParameterTypeNames = string.Join(", ", Method.GetParameters().Select(p => p.ParameterType.Name));
            var methodSignature = $"{Method.DeclaringType.Namespace}.{Method.DeclaringType.Name}.{Method.Name}({methodParameterTypeNames})";
            var logMessage = $"{methodSignature} - {this.stopwatch.Elapsed}";

            this.fileLogger.Log(logMessage);
        }
    }
}
