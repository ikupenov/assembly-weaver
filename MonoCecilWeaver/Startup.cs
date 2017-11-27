using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using CommandLine;
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
            var options = new Options();
            if (!Parser.Default.ParseArguments(args, options))
            {
                throw new ArgumentException("Invalid parameters.");
            }

            var backupAssemblyPath = CreateBackup(options.AssemblyPath);

            var assemblyWeaver = new AssemblyWeaver(options.AssemblyPath, options.DependencyDirectories);
            var assemblyResolver = new AssemblyResolver(backupAssemblyPath, options.DependencyDirectories);
            var definitionProvider = new DefinitionProvider(assemblyWeaver.AssemblyDefinition);

            if (options.ShouldEnableLogging)
            {
                SetupExceptionLogger(assemblyWeaver, assemblyResolver, definitionProvider.MethodDefinitions);
            }

            if (options.ShouldEnableProfiler)
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

        private static string CreateBackup(string filePath, bool overwrite = true)
        {
            var binPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().GetName().CodeBase);
            var backupFilePath = $"{Path.GetTempPath()}{Path.GetFileName(filePath)}.backup";

            File.Copy(filePath, backupFilePath, overwrite);

            return backupFilePath;
        }
    }
}
