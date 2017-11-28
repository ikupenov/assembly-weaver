using System;

namespace MonoCecilWeaver.Target.Attributes
{
    /// <summary>
    /// Disables exception logging for the given target.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method | AttributeTargets.Constructor, AllowMultiple = false, Inherited = true)]
    public class DisableExceptionLoggingAttribute : Attribute
    {
    }
}
