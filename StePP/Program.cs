using System;
using System.IO;
using CommandLine;
using StePP.Config;
using StePP.Runner;
using YamlDotNet.Core;
using Parser = CommandLine.Parser;

namespace StePP
{
    // ReSharper disable once ClassNeverInstantiated.Global
    internal class Program
    {
        private static void Main(string[] args)
        {
            Parser.Default.ParseArguments<Cli>(args)
                .WithParsed(Run);
        }

        private static void Run(Cli cli)
        {
            var originalWorkingDirectory = Directory.GetCurrentDirectory();
            try
            {
                Console.WriteLine("Reading config from: " + cli.ConfigFile);
                var config = new Reader(cli.ConfigFile).ParseConfig();
                ChangeCurrentDirectory(cli.ConfigFile);
                Console.WriteLine(config.Name + " v" + config.Version);
                var manager = new Manager(config.Steps, config.Actions, config.LogPath);
                manager.Start();
            }
            catch (ConfigParseException)
            {
                Console.WriteLine("Fix the Errors above, and re-run");
            }
            catch (YamlException e)
            {
                Console.WriteLine("Error in " + cli.ConfigFile);
                Console.WriteLine(e.Message);
            }
//            catch (Exception)
//            {
//                Console.WriteLine("There was an error");
//            }
            Directory.SetCurrentDirectory(originalWorkingDirectory);
            Console.ReadKey();
        }

        private static void ChangeCurrentDirectory(string configFilePath)
        {
            Directory.SetCurrentDirectory(Path.GetDirectoryName(Path.GetFullPath(configFilePath)));
        }
    }
}
