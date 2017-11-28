using System;
using System.Collections.Generic;
using MonoCecilWeaver.Core.Contexts;
using MonoCecilWeaver.Target;

namespace MonoCecilWeaver.Core
{
    /// <summary>
    /// Extension methods for the <see cref="MethodContext"/> type.
    /// </summary>
    public static class MethodContextExtensions
    {
        /// <summary>
        /// Iterates over a collection, catches the given <see cref="Exception"/> and handles it using the given <see cref="ExceptionHandler"/>.
        /// It does not rethrow the caught <see cref="Exception"/> and returns fake data instead.
        /// </summary>
        /// <typeparam name="TException">The <see cref="Exception"/> that will be handled.</typeparam>
        /// <typeparam name="THandler">The <see cref="ExceptionHandler"/> for the given exception.</typeparam>
        /// <returns>The current instance</returns>
        public static IEnumerable<MethodContext> Catch<TException, THandler>(this IEnumerable<MethodContext> values)
            where TException : Exception
            where THandler : ExceptionHandler, new()
        {
            foreach (var value in values)
            {
                value.Catch<TException, THandler>();
            }

            return values;
        }

        /// <summary>
        /// Iterates over a collection, catches the given <see cref="Exception"/> and handles it using the given <see cref="ExceptionHandler"/>.
        /// Unlike <seealso cref="MethodContextExtensions.Catch{TException, THandler}(IEnumerable{MethodContext})"/> 
        /// it rethrows the <see cref="Exception"/> after it has been handled.
        /// </summary>
        /// <typeparam name="TException">The <see cref="Exception"/> that will be handled.</typeparam>
        /// <typeparam name="THandler">The <see cref="ExceptionHandler"/> for the given exception.</typeparam>
        /// <returns>The current instance</returns>
        public static IEnumerable<MethodContext> Rethrow<TException, THandler>(this IEnumerable<MethodContext> values)
            where TException : Exception
            where THandler : ExceptionHandler, new()
        {
            foreach (var value in values)
            {
                value.Rethrow<TException, THandler>();
            }

            return values;
        }

        /// <summary>
        /// Iterates over a collection and inserts the given <see cref="PerformanceProfiler"/>. 
        /// Invokes the <see cref="PerformanceProfiler.Start"/> handler in the beggining 
        /// and the <see cref="PerformanceProfiler.Stop"/> handler in the end of the method.
        /// </summary>
        /// <returns>The current instance</returns>
        public static IEnumerable<MethodContext> Measure<TProfiler>(this IEnumerable<MethodContext> values)
            where TProfiler : PerformanceProfiler
        {
            foreach (var value in values)
            {
                value.Measure<TProfiler>();
            }

            return values;
        }
    }
}
