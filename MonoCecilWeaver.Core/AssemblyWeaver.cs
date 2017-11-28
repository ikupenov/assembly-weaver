using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Mono.Cecil;
using MonoCecilWeaver.Core.Contexts;

namespace MonoCecilWeaver.Core
{
    /// <summary>
    /// Wraps the logic of setupping <see cref="MethodContext"/> and saving changes to the given assembly.
    /// </summary>
    public class AssemblyWeaver
    {
        private readonly string assemblyPath;

        public AssemblyWeaver(string assemblyPath, IEnumerable<string> dependencyDirectories)
        {
            var assemblyResolver = new DefaultAssemblyResolver();
            foreach (var dependencyDirectory in dependencyDirectories)
            {
                assemblyResolver.AddSearchDirectory(dependencyDirectory);
            }

            var readerParameters = new ReaderParameters { AssemblyResolver = assemblyResolver };

            this.assemblyPath = assemblyPath;
            this.AssemblyDefinition = AssemblyDefinition.ReadAssembly(assemblyPath, readerParameters);
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
            this.AssemblyDefinition.Write(this.assemblyPath);
    }
}
