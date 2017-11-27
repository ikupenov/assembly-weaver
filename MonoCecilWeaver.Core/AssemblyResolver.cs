using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Mono.Cecil;

namespace MonoCecilWeaver.Core
{
    public class AssemblyResolver
    {
        private const string Mscorlib = "mscorlib";

        private const char GenericOpeneningTagBefore = '<';
        private const char GenericClosingTagBefore = '>';
        private const char GenericOpeningTagAfter = '[';
        private const char GenericClosingTagAfter = ']';
        private const char NestedClassSeparatorBefore = '/';
        private const char NestedClassSeparatorAfter = '+';

        public AssemblyResolver(string assemblyPath, IEnumerable<string> dependencyDirectories)
        {
            this.Assembly = Assembly.LoadFrom(assemblyPath);
            this.ReferencedAssemblies = GetReferencedAssemblies(this.Assembly, dependencyDirectories);

            this.ResolvedTypes = new Dictionary<string, Type>();
        }

        public Assembly Assembly { get; }

        public IEnumerable<Assembly> ReferencedAssemblies { get; }

        private IDictionary<string, Type> ResolvedTypes { get; }

        public Type GetType(TypeReference typeReference, bool throwError = false)
        {
            var typeReferenceName = typeReference.FullName;

            if (this.ResolvedTypes.ContainsKey(typeReferenceName))
            {
                return this.ResolvedTypes[typeReferenceName];
            }

            Type type = null;

            if (typeReference.IsGenericInstance)
            {
                type = GetGenericType((GenericInstanceType)typeReference, throwError);
            }
            else
            {
                type = GetType(typeReferenceName, throwError);
            }

            this.ResolvedTypes[typeReferenceName] = type;

            return type;
        }

        public Type GetType(string typeName, bool throwError = false)
        {
            typeName = typeName.Replace(GenericOpeneningTagBefore, GenericOpeningTagAfter);
            typeName = typeName.Replace(GenericClosingTagBefore, GenericClosingTagAfter);
            typeName = typeName.Replace(NestedClassSeparatorBefore, NestedClassSeparatorAfter);

            Type type = Type.GetType(typeName);

            if (type != null)
            {
                return type;
            }

            type = this.Assembly.GetType(typeName);

            if (type != null)
            {
                return type;
            }

            foreach (var referencedAssembly in this.ReferencedAssemblies)
            {
                type = referencedAssembly.GetType(typeName);

                if (type != null)
                {
                    return type;
                }
            }

            if (throwError)
            {
                throw new TypeLoadException($"Type [{typeName}] could not be loaded.");
            }

            return null;
        }

        public Type GetGenericType(GenericInstanceType genericInstanceType, bool throwError = false)
        {
            var genericType = GetType(genericInstanceType.GetElementType().FullName);
            var genericTypeArgs = new List<Type>();

            foreach (var parameter in genericInstanceType.GenericArguments.ToList())
            {
                if (parameter.IsGenericInstance)
                {
                    genericTypeArgs.Add(GetGenericType((GenericInstanceType)parameter, throwError));
                }
                else
                {
                    genericTypeArgs.Add(GetType(parameter.FullName, throwError));
                }
            }

            return genericType.MakeGenericType(genericTypeArgs.ToArray());
        }

        // TODO: Load assemblies recursively
        private IEnumerable<Assembly> GetReferencedAssemblies(Assembly assembly, IEnumerable<string> dependencyDirectories)
        {
            var referencedAssemblies = new List<Assembly>();

            foreach (var assemblyReferenceName in this.Assembly.GetReferencedAssemblies())
            {
                if (assemblyReferenceName.Name == Mscorlib)
                {
                    continue;
                }

                var doesExist = false;

                foreach (var dependencyDirectory in dependencyDirectories)
                {
                    var assemblyReferencePathDll = Path.Combine(dependencyDirectory, $"{assemblyReferenceName.Name}.dll");
                    var assemblyReferencePathExe = Path.Combine(dependencyDirectory, $"{assemblyReferenceName.Name}.exe");

                    if (TryLoad(assemblyReferenceName, out Assembly assemblyReference))
                    {
                        referencedAssemblies.Add(assemblyReference);
                        doesExist = true;
                    }
                    else if (File.Exists(assemblyReferencePathDll))
                    {
                        assemblyReference = Assembly.LoadFrom(assemblyReferencePathDll);
                        referencedAssemblies.Add(assemblyReference);
                        doesExist = true;
                    }
                    else if (File.Exists(assemblyReferencePathExe))
                    {
                        assemblyReference = Assembly.LoadFrom(assemblyReferencePathExe);
                        referencedAssemblies.Add(assemblyReference);
                        doesExist = true;
                    }
                }

                if (!doesExist)
                {
                    throw new FileNotFoundException($"Assembly {assemblyReferenceName.Name} not found.");
                }
            }

            return referencedAssemblies;
        }

        private bool TryLoad(AssemblyName assemblyName, out Assembly assembly)
        {
            assembly = null;

            try
            {
                assembly = Assembly.Load(assemblyName);
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
