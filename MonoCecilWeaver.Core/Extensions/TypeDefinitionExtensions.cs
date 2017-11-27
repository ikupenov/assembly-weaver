using System;
using System.Linq;
using Mono.Cecil;

namespace MonoCecilWeaver.Core
{
    public static class TypeDefinitionExtensions
    {
        public static Type GetType(this TypeDefinition value, AssemblyResolver assemblyResolver) =>
            assemblyResolver.GetType(value);

        public static bool IsAnonymousType(this TypeDefinition value) =>
            string.IsNullOrEmpty(value.Namespace);

        public static bool HasCustomAttribute<TAttribute>(this TypeDefinition value, AssemblyResolver assemblyResolver)
            where TAttribute : Attribute
        {
            if (assemblyResolver is null)
            {
                return value.HasCustomAttribute<TAttribute>();
            }

            var resolvedType = value.GetType(assemblyResolver);

            if (resolvedType is null)
            {
                return false;
            }

            return resolvedType.GetCustomAttributes(typeof(TAttribute), true).Any();
        }
    }
}
