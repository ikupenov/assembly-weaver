using System;
using System.Reflection;

namespace MonoCecilWeaver.Target
{
    public abstract class PerformanceProfiler
    {
        public PerformanceProfiler(MethodBase method)
        {
            if (method is null)
            {
                throw new ArgumentNullException();
            }

            this.Method = method;
        }

        protected MethodBase Method { get; }

        public static MethodBase StartHandler<TMeasurer>()
            where TMeasurer : PerformanceProfiler
        {
            return typeof(TMeasurer).GetMethod(nameof(Start));
        }

        public static MethodBase StopHandler<TMeasurer>()
            where TMeasurer : PerformanceProfiler
        {
            return typeof(TMeasurer).GetMethod(nameof(Stop));
        }

        public abstract void Start();

        public abstract void Stop();
    }
}
