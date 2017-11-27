using System;

namespace MonoCecilWeaver.Target.Attributes
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method | AttributeTargets.Constructor, AllowMultiple = false, Inherited = true)]
    public class EnableExceptionLoggingAttribute : Attribute
    {
    }
}
