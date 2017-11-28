using System;

namespace MonoCecilWeaver.Target.Attributes
{
    /// <summary>
    /// Enables performance profiler for the given target.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method | AttributeTargets.Constructor, AllowMultiple = false, Inherited = true)]
    public class EnablePerformanceProfilerAttribute : Attribute
    {
    }
}
