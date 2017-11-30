using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using Mono.Cecil;
using Mono.Cecil.Pdb;
using MonoCecilWeaver.Core.Contexts;

namespace MonoCecilWeaver.Core
{
    /// <summary>
    /// Wraps the logic of setupping <see cref="MethodContext"/> and saving changes to the given assembly.
    /// </summary>
    public class AssemblyWeaver
    {
        private const string AssemblyBackupSuffix = "backup";
        private const string PdbExtensionSuffix = "pdb";

        private readonly string assemblyPath;
        private readonly ReaderParameters readerParameters;
        private readonly WriterParameters writerParameters;

        public AssemblyWeaver(string assemblyPath, IEnumerable<string> searchDirectories, bool createBackup = true)
        {
            if (createBackup)
            {
                BackupAssembly(assemblyPath);
            }

            TryGetPdbPath(assemblyPath, out string pdbPath);

            this.assemblyPath = assemblyPath;
            this.readerParameters = CreateDefaultReaderParameters(searchDirectories, pdbPath);
            this.writerParameters = CreateDefaultWriterParameters(pdbPath);

            this.AssemblyDefinition = AssemblyDefinition.ReadAssembly(assemblyPath, this.readerParameters);
        }

        public AssemblyDefinition AssemblyDefinition { get; }

        /// <summary>
        /// Wraps up the <see cref="MethodDefinition"/> and its <see cref="ModuleDefinition"/>,
        /// allowing for easy manipulation of the <see cref="MethodDefinition"/> object.
        /// </summary>
        public MethodContext Setup(Expression<Action> expression)
        {
            var methodCall = expression.Body as MethodCallExpression;
            var methodDeclaringType = methodCall.Method.DeclaringType;

            var methodDefinition = this.AssemblyDefinition.MainModule.Types
                .Single(t => t.Name == methodDeclaringType.Name)
                .Methods
                .Single(m => m.Name == methodCall.Method.Name);

            return new MethodContext(this.AssemblyDefinition.MainModule, methodDefinition);
        }

        /// <summary>
        /// Wraps up the <see cref="MethodDefinition"/> and its <see cref="ModuleDefinition"/>,
        /// allowing for easy manipulation of the <see cref="MethodDefinition"/> object.
        /// </summary>
        public MethodContext Setup(MethodDefinition methodDefinition) =>
             new MethodContext(this.AssemblyDefinition.MainModule, methodDefinition);

        /// <summary>
        /// Save changes to the <see cref="Mono.Cecil.AssemblyDefinition"/>.
        /// </summary>
        public void Reweave() =>
            this.AssemblyDefinition.Write(this.assemblyPath, this.writerParameters);

        private static void BackupAssembly(string assemblyPath, bool overwrite = true)
        {
            var binPath = Path.GetDirectoryName(assemblyPath);

            var assemblyBackupPath = $"{binPath}{Path.DirectorySeparatorChar}{Path.GetFileName(assemblyPath)}.{AssemblyBackupSuffix}";
            File.Copy(assemblyPath, assemblyBackupPath, overwrite);

            if (TryGetPdbPath(assemblyPath, out string pdbPath))
            {
                var pdbBackupPath = $"{binPath}{Path.DirectorySeparatorChar}{Path.GetFileName(pdbPath)}.{AssemblyBackupSuffix}";
                File.Copy(pdbPath, pdbBackupPath, overwrite);
            }
        }

        private static bool TryGetPdbPath(string assemblyPath, out string pdbPath)
        {
            var possibePdbPath = Path.ChangeExtension(assemblyPath, PdbExtensionSuffix);

            if (File.Exists(possibePdbPath))
            {
                pdbPath = possibePdbPath;
                return true;
            }

            pdbPath = null;
            return false;
        }

        private ReaderParameters CreateDefaultReaderParameters(IEnumerable<string> searchDirectories, string pdbPath = null)
        {
            var assemblyResolver = new DefaultAssemblyResolver();
            foreach (var searchDirectory in searchDirectories)
            {
                if (!string.IsNullOrEmpty(searchDirectory))
                {
                    assemblyResolver.AddSearchDirectory(searchDirectory);
                }
            }

            var readerParameters = new ReaderParameters
            {
                AssemblyResolver = assemblyResolver
            };

            if (!string.IsNullOrEmpty(pdbPath))
            {
                var pdbReaderProvider = new PdbReaderProvider();
                readerParameters.SymbolReaderProvider = pdbReaderProvider;
                readerParameters.ReadSymbols = true;
            }

            return readerParameters;
        }

        private WriterParameters CreateDefaultWriterParameters(string pdbPath = null)
        {
            var writerParameters = new WriterParameters();

            if (!string.IsNullOrEmpty(pdbPath))
            {
                var pdbWriterProvider = new PdbWriterProvider();
                writerParameters.SymbolWriterProvider = pdbWriterProvider;
                writerParameters.WriteSymbols = true;
            }

            return writerParameters;
        }
    }
}
