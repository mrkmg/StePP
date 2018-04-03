using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using StePP.Config;
using Action = StePP.Config.Action;

namespace StePP.Runner
{
    public class StepRunner
    {
        private readonly OutputLogger _output;
        private readonly OutputLogger _managerOutput;
        private readonly string _stepName;
        private readonly Step _step;
        private readonly Dictionary<string, Action> _actions;
        private bool _killed;
        private ActionRunner _currentActionRunner;

        public StepRunner(string stepName, Step step, Dictionary<string, Action> actions, OutputLogger managerOutput, OutputLogger output)
        {
            _stepName = stepName;
            _step = step;
            _actions = actions;
            _managerOutput = managerOutput;
            _output = output;
        }

        public async Task<bool> Run()
        {
            _managerOutput.WriteLine("Starting Step: " + _stepName);

            var result = await RunActions();
            Cleanup();
            if (!result)
            {
                _managerOutput.WriteLine("Failed Step: " + _stepName);
                return false;
            }

            _managerOutput.WriteLine("Finished Step: " + _stepName);
            return true;
        }

        private async Task<bool> RunActions()
        {
            foreach (var actionName in _step.Actions)
            {
                _managerOutput.WriteLine("Starting Action: "  + _stepName + " - " + actionName);

                var result = await RunAction(actionName);
                if (!result)
                {
                    _managerOutput.WriteLine("Failed Action: "  + _stepName + " - " + actionName);

                    return false;
                }

                _managerOutput.WriteLine("Finished Action: "  + _stepName + " - " + actionName);
            }

            return true;
        }

        private async Task<bool> RunAction(string actionName)
        {
            if (_killed) return false;
            var outputStream = _step.LogPath == "-" ?
                new OutputLogger(_output, _stepName + " (" + actionName + ")") :
                new OutputLogger(_step.LogPath, _stepName + " (" + actionName + ")");

            _currentActionRunner = new ActionRunner(_actions[actionName], outputStream);
            var result = await _currentActionRunner.Run();

            if (_step.LogPath != "-") outputStream.Close();

            return result;
        }

        public void Kill()
        {
            _killed = true;
            _currentActionRunner?.Kill();
        }

        private void Cleanup()
        {
            _currentActionRunner = null;
        }
    }
}
