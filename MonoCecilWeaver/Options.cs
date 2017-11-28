using CommandLine;

namespace MonoCecilWeaver
{
    /// <summary>
    /// Maps input parameters.
    /// </summary>
    internal class Options
    {
        [Option('a', "assemblyPath", Required = true, HelpText = "Assembly to weave.")]
        public string AssemblyPath { get; set; }

        [Option('l', "enableLogging", DefaultValue = true, HelpText = "Enable exception logging.")]
        public bool ShouldEnableLogging { get; set; }

        [Option('p', "enableProfiler", DefaultValue = false, HelpText = "Enable performance profiler.")]
        public bool ShouldEnableProfiler { get; set; }

        [OptionArray('d', "dependencyDirectories", HelpText = "Assembly dependency directories.")]
        public string[] DependencyDirectories { get; set; }
    }
}
