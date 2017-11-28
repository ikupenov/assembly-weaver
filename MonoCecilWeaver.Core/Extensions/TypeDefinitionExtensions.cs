using System;
using System.Linq;
using Mono.Cecil;

namespace MonoCecilWeaver.Core
{
    /// <summary>
    /// Extension methods for the <see cref="TypeDefinition"/> type.
    /// </summary>
    public static class TypeDefinitionExtensions
    {
        /// <summary>
        /// Gets <see cref="Type"/> for the given <see cref="TypeDefinition"/>.
        /// </summary>
        /// <returns>The <see cref="Type"/> for the given <see cref="TypeDefinition"/></returns>
        public static Type GetType(this TypeDefinition value, AssemblyResolver assemblyResolver) =>
            assemblyResolver.GetType(value);

        public static bool IsAnonymousType(this TypeDefinition value) =>
            string.IsNullOrEmpty(value.Namespace);

        /// <summary>
        /// Checks whether the <see cref="TypeDefinition"/> has the given <see cref="Attribute"/>.
        /// If <paramref name="assemblyResolver"/> is null, the inheritence tree will not be traversed.
        /// </summary>
        /// <returns>Whether the <see cref="TypeDefinition"/> has the given <see cref="Attribute"/></returns>
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
