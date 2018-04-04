using System.Collections.Generic;
using System.Threading.Tasks;
using StePP.Config;

namespace StePP.Runner
{
    public class StepRunner
    {
        private readonly Dictionary<string, Action> _actions;
        private readonly OutputLogger _managerOutput;
        private readonly OutputLogger _output;
        private readonly Step _step;
        private readonly string _stepName;
        private OutputLogger _currentActionOutput;
        private ActionRunner _currentActionRunner;
        private bool _killed;

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
            _managerOutput.WriteLine("Starting " + _stepName);

            var result = await RunActions();
            Cleanup();
            if (!result)
            {
                _managerOutput.WriteLine("Failed " + _stepName);
                return false;
            }

            _managerOutput.WriteLine("Finished " + _stepName);
            return true;
        }

        public void Kill()
        {
            if (!_step.CanBeKilled) return;

            _killed = true;
            _currentActionRunner?.Kill();
        }

        private async Task<bool> RunActions()
        {
            foreach (var actionName in _step.Actions)
            {
                var stepActionName = GetStepActionString(actionName);

                _managerOutput.WriteLine("Starting " + stepActionName);

                SetupNextAction(actionName);
                var result = await RunCurrentAction();
                CleanupAction();

                if (!result)
                {
                    _managerOutput.WriteLine("Failed " + stepActionName);
                    if (!_actions[actionName].IgnoreFailure) return false;
                }
                else
                {
                    _managerOutput.WriteLine("Finished " + stepActionName);
                }
            }

            return true;
        }

        private void SetupNextAction(string actionName)
        {
            if (_killed) return;

            SetupActionOutput(actionName);
            _currentActionRunner = new ActionRunner(_actions[actionName], _currentActionOutput);
        }

        private async Task<bool> RunCurrentAction() => await _currentActionRunner.Run();

        private void CleanupAction()
        {
            _currentActionRunner = null;
            CleanupActionOutput();
        }

        private void Cleanup() => _currentActionRunner = null;

        private void SetupActionOutput(string actionName)
        {
            var stepActionName = GetStepActionString(actionName);
            _currentActionOutput = _step.LogPath == "-"
                ? new OutputLogger(_output, stepActionName)
                : new OutputLogger(_step.LogPath, stepActionName);
        }

        private void CleanupActionOutput()
        {
            if (_step.LogPath != "-") _currentActionOutput.Close();

            _currentActionOutput = null;
        }

        private string GetStepActionString(string actionName) => _stepName + "(" + actionName + ")";
    }
}
