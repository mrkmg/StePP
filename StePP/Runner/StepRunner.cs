using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using StePP.Config;
using Action = StePP.Config.Action;

namespace StePP.Runner
{
    public class StepRunner
    {
        private bool _killed;
        private readonly string _stepName;
        private readonly Step _step;
        private readonly Dictionary<string, Action> _actions;
        private ActionRunner _currentActionRunner;

        public StepRunner(string stepName, Step step, Dictionary<string, Action> actions)
        {
            _stepName = stepName;
            _step = step;
            _actions = actions;
        }

        public async Task<bool> Run()
        {
            Console.WriteLine("Manager | Starting Step: " + _stepName);
            foreach (var actionName in _step.Actions)
            {
                if (_killed) return false;
                Console.WriteLine("Manager | Starting Action: "  + _stepName + " - " + actionName);
                var result = await RunAction(actionName);
                if (_killed) return false;
                if (!result)
                {
                    Console.WriteLine("Manager | Failed Action: "  + _stepName + " - " + actionName);
                    Console.WriteLine("Manager | Failed Step: " + _stepName);
                    _currentActionRunner = null;
                    return false;
                }
                Console.WriteLine("Manager | Finished Action: "  + _stepName + " - " + actionName);
            }
            Console.WriteLine("Manager | Finished Step: " + _stepName);
            _currentActionRunner = null;
            return true;
        }

        private async Task<bool> RunAction(string actionName)
        {
            var outputStream = new OutputLogger(string.IsNullOrEmpty(_step.LogPath) ? "-" : _step.LogPath, _stepName + " " + actionName + " | ");
            _currentActionRunner = new ActionRunner(_actions[actionName], outputStream);
            var result = await _currentActionRunner.Run();
            outputStream.Close();
            return result;
        }

        public void Kill()
        {
            _killed = true;
            _currentActionRunner?.Kill();
        }
    }
}
