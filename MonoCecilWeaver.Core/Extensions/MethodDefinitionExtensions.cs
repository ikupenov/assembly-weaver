using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Mono.Cecil;
using MonoCecilWeaver.Core.Contexts;
using MonoCecilWeaver.Target.Attributes;

namespace MonoCecilWeaver.Core
{
    /// <summary>
    /// Extension methods for the <see cref="MethodDefinition"/> type.
    /// </summary>
    public static class MethodDefinitionExtensions
    {
        /// <summary>
        /// Checks whether the given <see cref="MethodDefinition"/> is Void.
        /// </summary>
        /// <returns>Whether the given <see cref="MethodDefinition"/> is Void</returns>
        public static bool IsVoid(this MethodDefinition value) =>
            value.ReturnType.FullName == "System.Void";

        /// <summary>
        /// Checks whether the given <see cref="MethodDefinition"/> is anonymous.
        /// </summary>
        /// <returns>Whether the given <see cref="MethodDefinition"/> is anonymous.</returns>
        public static bool IsAnonymousMethod(this MethodDefinition value) =>
            value.DeclaringType.IsAnonymousType();

        /// <summary>
        /// Gets the types of the method's parameters.
        /// </summary>
        /// <returns>The types of the method's parameters.</returns>
        public static IEnumerable<Type> GetTypeParameters(this MethodDefinition value, AssemblyResolver assemblyResolver) =>
            value.Parameters.Select(p => assemblyResolver.GetType(p.ParameterType, true));

        /// <summary>
        /// Checks whether the given method or its declaring type have the <see cref="EnableExceptionLoggingAttribute"/> 
        /// or the <see cref="DisableExceptionLoggingAttribute"/> attribute, searching the inheritence tree.
        /// </summary>
        /// <returns>Whether exception logging should be enabled for the given method.</returns>
        public static bool ShouldEnableLogging(this MethodDefinition value, AssemblyResolver assemblyResolver) =>
            value.ShouldEnable<EnableExceptionLoggingAttribute, DisableExceptionLoggingAttribute>(assemblyResolver);

        /// <summary>
        /// Checks whether the given method or its declaring type have the <see cref="EnablePerformanceProfilerAttribute"/> 
        /// or the <see cref="DisablePerformanceProfilerAttribute"/> attribute, searching the inheritence tree.
        /// </summary>
        /// <returns>Whether performance profiler should be enabled for the given method.</returns>
        public static bool ShouldEnableProfiler(this MethodDefinition value, AssemblyResolver assemblyResolver) =>
            value.ShouldEnable<EnablePerformanceProfilerAttribute, DisablePerformanceProfilerAttribute>(assemblyResolver);

        /// <summary>
        /// Gets the <see cref="MethodBase" for the given <see cref="MethodDefinition"/>/>
        /// </summary>
        /// <returns>The <see cref="MethodBase" for the given <see cref="MethodDefinition"/>/></returns>
        public static MethodBase GetMethodBase(this MethodDefinition value, AssemblyResolver assemblyResolver)
        {
            if (value.IsAnonymousMethod())
            {
                return null;
            }

            var declaringType = value.DeclaringType.GetType(assemblyResolver);

            var bindingFlags = BindingFlags.Public | BindingFlags.NonPublic;
            bindingFlags |= value.IsStatic ? BindingFlags.Static : BindingFlags.Instance;

            var methodParameters = value.GetTypeParameters(assemblyResolver).ToArray();

            if (value.IsConstructor)
            {
                return declaringType?.GetConstructor(bindingFlags, null, methodParameters, null);
            }
            else
            {
                return declaringType?.GetMethod(value.Name, bindingFlags, null, methodParameters, null);
            }
        }

        /// <summary>
        /// Checks whether the <see cref="MethodDefinition"/> has the given <see cref="Attribute"/>.
        /// If <paramref name="assemblyResolver"/> is null, the inheritence tree will not be traversed.
        /// </summary>
        /// <returns>Whether the <see cref="MethodDefinition"/> has the given <see cref="Attribute"/></returns>
        public static bool HasCustomAttribute<TAttribute>(this MethodDefinition value, AssemblyResolver assemblyResolver)
            where TAttribute : Attribute
        {
            if (assemblyResolver is null)
            {
                return value.HasCustomAttribute<TAttribute>();
            }

            var methodBase = value.GetMethodBase(assemblyResolver);

            if (methodBase is null)
            {
                return false;
            }

            return methodBase.GetCustomAttributes(typeof(TAttribute), true).Any();
        }

        /// <summary>
        /// Creates the <see cref="MethodContext"/> for the given <see cref="MethodDefinition"/>.
        /// </summary>
        /// <returns>The <see cref="MethodContext"/></returns>
        public static IEnumerable<MethodContext> Setup(this IEnumerable<MethodDefinition> values, AssemblyWeaver assemblyWeaver)
        {
            var methodContexts = new List<MethodContext>();

            foreach (var value in values)
            {
                methodContexts.Add(assemblyWeaver.Setup(value));
            }

            return methodContexts;
        }

        private static bool ShouldEnable<TEnable, TDisable>(this MethodDefinition value, AssemblyResolver assemblyResolver)
            where TEnable : Attribute
            where TDisable : Attribute
        {
            var methodHasDisableAttribute = value.HasCustomAttribute<TDisable>(assemblyResolver);
            if (methodHasDisableAttribute)
            {
                return false;
            }

            var parentHasEnableAttribute = value.DeclaringType.HasCustomAttribute<TEnable>(assemblyResolver);
            var methodHasEnableAttribute = value.HasCustomAttribute<TEnable>(assemblyResolver);

            return parentHasEnableAttribute || methodHasEnableAttribute;
        }
    }
}
