using System;
using System.Reflection;

namespace MonoCecilWeaver.Target
{
    /// <summary>
    /// Measures the performance of a method.
    /// </summary>
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

        /// <summary>
        /// Returns the <see cref="MethodBase"/> of the implemented <see cref="Start"/> 
        /// method for the given derived type.
        /// </summary>
        /// <returns><see cref="Start"/></returns>
        public static MethodBase StartHandler<TMeasurer>()
            where TMeasurer : PerformanceProfiler
        {
            return typeof(TMeasurer).GetMethod(nameof(Start));
        }

        /// <summary>
        /// Returns the <see cref="MethodBase"/> of the implemented <see cref="Stop"/> 
        /// method for the given derived type.
        /// </summary>
        /// <returns><see cref="Stop"/></returns>
        public static MethodBase StopHandler<TMeasurer>()
            where TMeasurer : PerformanceProfiler
        {
            return typeof(TMeasurer).GetMethod(nameof(Stop));
        }

        /// <summary>
        /// Invoked in the beginning of a method.
        /// </summary>
        public abstract void Start();

        /// <summary>
        /// Invoked in the end of a method.
        /// </summary>
        public abstract void Stop();
    }
}
