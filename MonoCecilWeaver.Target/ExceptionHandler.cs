using System;
using System.Reflection;

namespace MonoCecilWeaver.Target
{
    /// <summary>
    /// Handles an <see cref="Exception"/> that has been thrown.
    /// </summary>
    public abstract class ExceptionHandler
    {
        /// <summary>
        /// Returns the <see cref="MethodBase"/> of the implemented <see cref="Handle(Exception, object, MethodBase)"/> 
        /// method for the given derived type.
        /// </summary>
        /// <returns><see cref="Handle(Exception, object, MethodBase)"/></returns>
        public static MethodBase MethodHandler<THandler>()
            where THandler : ExceptionHandler
        {
            return typeof(THandler).GetMethod(nameof(Handle));
        }

        /// <summary>
        /// Invoked when an <see cref="Exception"/> occurs. Handles the thrown <see cref="Exception"/>.
        /// </summary>
        /// <param name="ex">The <see cref="Exception"/> that needs to be handled.</param>
        /// <param name="instance">The instance of the current method or <see langword="null"/> if static.</param>
        /// <param name="method">The current method.</param>
        public abstract void Handle(Exception ex, object instance, MethodBase method);
    }
}
