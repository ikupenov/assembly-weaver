using System;
using System.Linq;
using Mono.Cecil;

namespace MonoCecilWeaver.Core
{
    public static class ICustomAttributeProviderExtensions
    {
        public static bool HasCustomAttribute<TAttribute>(this ICustomAttributeProvider attributeProvider)
            where TAttribute : Attribute =>
                attributeProvider
                    .CustomAttributes
                    .Any(a => a.AttributeType.FullName == typeof(TAttribute).FullName);

        public static bool HasCustomAttribute(this ICustomAttributeProvider attributeProvider, string attributeName) =>
            attributeProvider
                .CustomAttributes
                .Any(a => a.AttributeType.FullName == attributeName);
    }
}