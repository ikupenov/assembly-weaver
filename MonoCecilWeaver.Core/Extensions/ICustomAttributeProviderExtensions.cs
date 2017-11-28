using System;
using System.Linq;
using Mono.Cecil;

namespace MonoCecilWeaver.Core
{
    /// <summary>
    /// Extension methods for the <see cref="ICustomAttributeProvider"/> type.
    /// </summary>
    public static class ICustomAttributeProviderExtensions
    {
        /// <summary>
        /// Checks whether the <see cref="ICustomAttributeProvider"/> has the given <see cref="Attribute"/>.
        /// It does not traverse the inheritence tree.
        /// </summary>
        /// <returns>Whether the <see cref="ICustomAttributeProvider"/> has the given <see cref="Attribute"/></returns>
        public static bool HasCustomAttribute<TAttribute>(this ICustomAttributeProvider attributeProvider)
            where TAttribute : Attribute =>
                attributeProvider
                    .CustomAttributes
                    .Any(a => a.AttributeType.FullName == typeof(TAttribute).FullName);

        /// <summary>
        /// Checks whether the <see cref="ICustomAttributeProvider"/> has the given <see cref="Attribute"/>
        /// by comparing the <see cref="Attribute"/> name with the <paramref name="attributeName"/>. 
        /// It does not traverse the inheritence tree.
        /// </summary>
        /// <returns>Whether the <see cref="ICustomAttributeProvider"/> has the given <see cref="Attribute"/></returns>
        public static bool HasCustomAttribute(this ICustomAttributeProvider attributeProvider, string attributeName) =>
            attributeProvider
                .CustomAttributes
                .Any(a => a.AttributeType.FullName == attributeName);
    }
}