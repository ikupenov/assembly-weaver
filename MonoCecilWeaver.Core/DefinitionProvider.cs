using System.Collections.Generic;
using System.Linq;
using Mono.Cecil;
using Mono.Cecil.Rocks;

namespace MonoCecilWeaver.Core
{
    public class DefinitionProvider
    {
        private readonly AssemblyDefinition assemblyDefinition;

        private IEnumerable<MethodDefinition> methodDefinitions;

        public DefinitionProvider(AssemblyDefinition assemblyDefinition)
        {
            this.assemblyDefinition = assemblyDefinition;
        }

        public IEnumerable<MethodDefinition> MethodDefinitions
        {
            get
            {
                if (this.methodDefinitions is null)
                {
                    var methods = this.assemblyDefinition.Modules
                        .SelectMany(ModuleDefinitionRocks.GetAllTypes)
                        .SelectMany(t => t.Methods);

                    this.methodDefinitions = methods;
                }

                return this.methodDefinitions;
            }
        }
    }
}
