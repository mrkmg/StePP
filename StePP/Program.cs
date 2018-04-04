using System;
using System.Collections.Generic;
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
        private static string _originalWorkingDirectory;
        private static Cli _cli;
        private static Root _rootConfig;

        private static void Main(string[] args)
        {
            try
            {
                ParseCli(args);
                ReadConfig();
                MoveToWorkingDirectory();
                StartManager();
            }
            catch (ConfigParseException)
            {
                Console.WriteLine("Fix the Errors above, and re-run");
            }
            catch (YamlException e)
            {
                Console.WriteLine("Error in " + _cli.ConfigFile);
                Console.WriteLine(e.Message);
                Console.WriteLine(e.InnerException.Message);
            }
            finally
            {
                MoveToOriginalWorkingDirectory();
                Console.ReadKey(); // For testing only TODO
            }
        }

        private static void StartManager()
        {
            Console.WriteLine(_rootConfig.Name + " v" + _rootConfig.Version);
            var manager = new Manager(_rootConfig.Steps, _rootConfig.Actions, _rootConfig.LogPath);
            manager.Start();
        }

        private static void ReadConfig() => _rootConfig = new Reader(_cli.ConfigFile).ParseConfig();

        private static void ParseCli(IEnumerable<string> args) => Parser.Default.ParseArguments<Cli>(args).WithParsed(cli => _cli = cli);

        private static void MoveToWorkingDirectory()
        {
            _originalWorkingDirectory = Directory.GetCurrentDirectory();
            Directory.SetCurrentDirectory(Path.GetDirectoryName(Path.GetFullPath(_cli.ConfigFile)));
        }

        private static void MoveToOriginalWorkingDirectory()
        {
            if (_originalWorkingDirectory == null) return;
            Directory.SetCurrentDirectory(_originalWorkingDirectory);
        }
    }
}
