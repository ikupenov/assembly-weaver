using System;
using System.Collections.Generic;
using MonoCecilWeaver.Core.Contexts;
using MonoCecilWeaver.Target;

namespace MonoCecilWeaver.Core
{
    public static class MethodContextExtensions
    {
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
