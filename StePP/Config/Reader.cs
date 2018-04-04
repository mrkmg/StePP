using System;
using System.Collections.Generic;
using System.IO;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace StePP.Config
{
    public class Reader
    {
        private readonly string _configPath;

        public Reader(string configPath) => _configPath = configPath;

        public Root ParseConfig()
        {
            var fileData = File.ReadAllText(_configPath);
            var deserializer = new DeserializerBuilder().WithNamingConvention(new CamelCaseNamingConvention()).Build();
            var config = deserializer.Deserialize<Root>(fileData);

            var isMissingExecutables = CheckMissingExecutables(config);
            var isMissingActions = CheckMissingActions(config);
            var isMissingSteps = CheckMissingSteps(config);
            var hasCircularPrerequisites = CheckForCircularPrerequisites(config);

            if (isMissingExecutables || isMissingActions || isMissingSteps || hasCircularPrerequisites) throw new ConfigParseException();

            return config;
        }

        private static bool CheckMissingExecutables(Root rootConfig)
        {
            var missingExecutables = new List<string>();
            foreach (var actionEntry in rootConfig.Actions)
            {
                var actionName = actionEntry.Key;
                var action = actionEntry.Value;

                if (action.Executable == null) missingExecutables.Add(actionName);
            }

            if (missingExecutables.Count <= 0) return false;

            Console.WriteLine("The following actions are missing an executable: " + string.Join(", ", missingExecutables));
            return true;
        }

        private static bool CheckMissingActions(Root rootConfig)
        {
            var missingActions = new List<string>();
            foreach (var stepEntry in rootConfig.Steps)
            {
                var step = stepEntry.Value;
                foreach (var actionName in step.Actions)
                    if (!rootConfig.Actions.ContainsKey(actionName) && !missingActions.Contains(actionName))
                        missingActions.Add(actionName);
            }

            if (missingActions.Count <= 0) return false;
            Console.WriteLine("The following actions are missing but referenced in a step: " + string.Join(", ", missingActions));
            return true;
        }

        private static bool CheckMissingSteps(Root rootConfig)
        {
            var missingSteps = new List<string>();
            foreach (var stepEntry in rootConfig.Steps)
            {
                var step = stepEntry.Value;
                foreach (var prerequisiteName in step.Prerequisites)
                    if (!rootConfig.Steps.ContainsKey(prerequisiteName) && !missingSteps.Contains(prerequisiteName))
                        missingSteps.Add(prerequisiteName);
            }

            if (missingSteps.Count <= 0) return false;
            Console.WriteLine("The following steps are missing but referenced in a prerequisites: " + string.Join(", ", missingSteps));
            return true;
        }

        private static bool CheckForCircularPrerequisites(Root rootConfig)
        {
            var circularSteps = new List<string>();

            foreach (var stepEntry in rootConfig.Steps)
            {
                var stepName = stepEntry.Key;
                var step = stepEntry.Value;

                var currentToCheck = new List<string>(step.Prerequisites);
                var currentChecked = new List<string>();

                while (currentToCheck.Count > 0)
                {
                    var name = currentToCheck[0];
                    currentToCheck.RemoveAt(0);

                    if (currentChecked.Contains(name)) continue;

                    if (name == stepName)
                    {
                        circularSteps.Add(stepName);
                        break;
                    }

                    currentChecked.Add(name);

                    currentToCheck.AddRange(rootConfig.Steps[name].Prerequisites);
                }
            }

            if (circularSteps.Count <= 0) return false;
            Console.WriteLine("The following steps have a circular reference: " + string.Join(", ", circularSteps));
            return true;
        }
    }

    public class ConfigParseException : Exception
    {
    }
}
