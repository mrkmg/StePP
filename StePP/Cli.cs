// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable ClassNeverInstantiated.Global
using CommandLine;

namespace StePP
{
    public class Cli
    {
        [Option('c', "config", Required = true, HelpText = "Path to config file")]
        public string ConfigFile { get; set; }
    }
}
