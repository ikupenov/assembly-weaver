using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Mono.Cecil;
using MonoCecilWeaver.Core.Contexts;
using MonoCecilWeaver.Target.Attributes;

namespace MonoCecilWeaver.Core
{
    public static class MethodDefinitionExtensions
    {
        public static bool IsVoid(this MethodDefinition value) =>
            value.ReturnType.FullName == "System.Void";

        public static bool IsAnonymousMethod(this MethodDefinition value) =>
            value.DeclaringType.IsAnonymousType();

        public static IEnumerable<Type> GetTypeParameters(this MethodDefinition value, AssemblyResolver assemblyResolver) =>
            value.Parameters.Select(p => assemblyResolver.GetType(p.ParameterType, true));

        public static bool ShouldEnableLogging(this MethodDefinition value, AssemblyResolver assemblyResolver) =>
            value.ShouldEnable<EnableExceptionLoggingAttribute, DisableExceptionLoggingAttribute>(assemblyResolver);

        public static bool ShouldEnableProfiler(this MethodDefinition value, AssemblyResolver assemblyResolver) =>
            value.ShouldEnable<EnablePerformanceProfilerAttribute, DisablePerformanceProfilerAttribute>(assemblyResolver);

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
