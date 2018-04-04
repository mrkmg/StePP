using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CSharpx;
using StePP.Config;
using Action = StePP.Config.Action;

namespace StePP.Runner
{
    public class Manager
    {
        private readonly Dictionary<string, Action> _actions;
        private readonly List<string> _finishSteps = new List<string>();
        private readonly OutputLogger _managerOutput;
        private readonly OutputLogger _output;
        private readonly Dictionary<string, Task<bool>> _runningTasks = new Dictionary<string, Task<bool>>();
        private readonly Dictionary<string, StepRunner> _stepRunners = new Dictionary<string, StepRunner>();
        private readonly Dictionary<string, Step> _steps;

        public Manager(Dictionary<string, Step> steps, Dictionary<string, Action> actions, string logPath)
        {
            _steps = steps;
            _actions = actions;
            _output = new OutputLogger(logPath);
            _managerOutput = new OutputLogger(_output, "Manager");
        }

        public void Start()
        {
            try
            {
                _managerOutput.WriteLine("Process Started");
                while (true)
                {
                    RunSteps();
                    if (_runningTasks.Count == 0) break;
                    CheckSteps();
                    WaitAnyTasks();
                }

                _managerOutput.WriteLine("All Complete");
            }
            catch (StepFailedException)
            {
                KillAllRunningSteps();
                WaitAllTasks();
                _managerOutput.WriteLine("Process Failed");
            }

            Cleanup();
        }

        private void Cleanup()
        {
            _output.Close();
            _managerOutput.Close();
        }

        private void KillAllRunningSteps() => _stepRunners.ForEach(e => e.Value.Kill());

        private void WaitAllTasks() => Task.WaitAll(_runningTasks.Values.ToArray<Task>());

        private void WaitAnyTasks() => Task.WaitAny(_runningTasks.Values.ToArray<Task>());

        private void CheckSteps()
        {
            var finished = new List<string>();
            foreach (var taskEntry in _runningTasks)
            {
                var stepName = taskEntry.Key;
                var task = taskEntry.Value;
                var step = _steps[stepName];

                if (!task.IsCompleted) continue;
                if (!step.IgnoreFailure && !task.Result) throw new StepFailedException();
                finished.Add(stepName);
            }

            foreach (var stepName in finished)
            {
                _stepRunners.Remove(stepName);
                _runningTasks.Remove(stepName);
                _finishSteps.Add(stepName);
            }
        }

        private void RunSteps()
        {
            var runnableSteps = GetRunnableSteps();

            foreach (var stepName in runnableSteps)
            {
                var step = _steps[stepName];
                RunStep(stepName, step);
            }
        }

        private void RunStep(string stepName, Step step)
        {
            var runner = new StepRunner(stepName, step, _actions, _managerOutput, _output);
            _stepRunners.Add(stepName, runner);
            _runningTasks.Add(stepName, runner.Run());
        }

        private IEnumerable<string> GetRunnableSteps()
        {
            var runnableSteps = new List<string>();
            foreach (var entry in _steps)
            {
                var name = entry.Key;
                var step = entry.Value;

                if (StepAbleToRun(name, step)) runnableSteps.Add(name);
            }

            return runnableSteps;
        }

        private bool StepAbleToRun(string name, Step step)
        {
            if (_runningTasks.ContainsKey(name)) return false;
            if (_finishSteps.Contains(name)) return false;

            foreach (var prereqStep in step.Prerequisites)
                if (!_finishSteps.Contains(prereqStep))
                    return false;

            return true;
        }
    }

    public class StepFailedException : Exception
    {
    }
}
