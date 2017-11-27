using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Mono.Cecil;
using MonoCecilWeaver.Core;
using MonoCecilWeaver.Handlers;

namespace MonoCecilWeaver
{
    internal static class Startup
    {
        private const string TestMethodAttribute = "TestMethodAttribute";

        private static void Main(string[] args)
        {
            var assemblyPath = args[0];
            if (!File.Exists(assemblyPath))
            {
                throw new ArgumentException("The given assembly does not exist.");
            }

            if (!bool.TryParse(args[1], out var shouldEnableLogging))
            {
                throw new ArgumentException("The parameter must be either \"true\" or \"false\".");
            }

            if (!bool.TryParse(args[2], out var shouldEnableProfiler))
            {
                throw new ArgumentException("The parameter must be either \"true\" or \"false\".");
            }

            var dependenciesDirectories = args.Skip(3);
            if (dependenciesDirectories.Any(d => !Directory.Exists(d)))
            {
                throw new ArgumentException("One or more of the given search directories does not exist.");
            }

            var tempAssemblyPath = $"{Path.GetTempPath()}{Path.GetFileName(assemblyPath)}";
            File.Copy(assemblyPath, tempAssemblyPath, true);

            var assemblyWeaver = new AssemblyWeaver(assemblyPath, dependenciesDirectories);
            var assemblyResolver = new AssemblyResolver(tempAssemblyPath, dependenciesDirectories);
            var definitionProvider = new DefinitionProvider(assemblyWeaver.AssemblyDefinition);

            if (shouldEnableLogging)
            {
                SetupExceptionLogger(assemblyWeaver, assemblyResolver, definitionProvider.MethodDefinitions);
            }

            if (shouldEnableProfiler)
            {
                SetupPerformanceProfiler(assemblyWeaver, assemblyResolver, definitionProvider.MethodDefinitions);
            }

            assemblyWeaver.Reweave();
        }

        private static void SetupExceptionLogger(
            AssemblyWeaver assemblyWeaver,
            AssemblyResolver assemblyResolver,
            IEnumerable<MethodDefinition> methodDefinitions) =>
                methodDefinitions
                    .Where(m => m.ShouldEnableLogging(assemblyResolver) || m.HasCustomAttribute(TestMethodAttribute))
                    .Setup(assemblyWeaver)
                    .Rethrow<Exception, TestContextExceptionLogger>();

        private static void SetupPerformanceProfiler(
            AssemblyWeaver assemblyWeaver,
            AssemblyResolver assemblyResolver,
            IEnumerable<MethodDefinition> methodDefinitions) =>
                methodDefinitions
                    //.Where(m => m.ShouldEnableProfiler(assemblyResolver))
                    .Setup(assemblyWeaver)
                    .Measure<PerformanceLogger>();
    }
}
