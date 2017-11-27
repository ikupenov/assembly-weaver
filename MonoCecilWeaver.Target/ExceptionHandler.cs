using System;
using System.Reflection;

namespace MonoCecilWeaver.Target
{
    public abstract class ExceptionHandler
    {
        public static MethodBase MethodHandler<THandler>()
            where THandler : ExceptionHandler
        {
            return typeof(THandler).GetMethod(nameof(Handle));
        }

        public abstract void Handle(Exception ex, object instance, MethodBase method);
    }
}
