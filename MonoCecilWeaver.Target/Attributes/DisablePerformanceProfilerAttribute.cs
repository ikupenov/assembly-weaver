using System;

namespace MonoCecilWeaver.Target.Attributes
{
    /// <summary>
    /// Disables performance profiler for the given target.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method | AttributeTargets.Constructor, AllowMultiple = false, Inherited = true)]
    public class DisablePerformanceProfilerAttribute : Attribute
    {
    }
}
