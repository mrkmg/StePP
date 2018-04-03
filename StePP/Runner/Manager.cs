﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using StePP.Config;
using Action = StePP.Config.Action;

namespace StePP.Runner
{
    public class Manager
    {
        private readonly Dictionary<string, Step> _steps;
        private readonly Dictionary<string, Action> _actions;
        private readonly List<string> _finishSteps = new List<string>();
        private readonly Dictionary<string, StepRunner> _stepRunners = new Dictionary<string, StepRunner>();
        private readonly Dictionary<string, Task<bool>> _runningTasks = new Dictionary<string, Task<bool>>();

        public Manager(Dictionary<string, Step> steps, Dictionary<string, Action> actions)
        {
            _steps = steps;
            _actions = actions;
        }

        public void Start()
        {
            try
            {
                Console.WriteLine("Manager | Process Started");
                while (true)
                {
                    RunSteps();
                    ParseSteps();
                    WaitAnyTasks();
                    if (_runningTasks.Count == 0) break;
                }

                Console.WriteLine("Manager | All Complete");
            }
            catch (StepFailedException)
            {
                KillAllRunningSteps();
                WaitAllTasks();
                Console.WriteLine("Manager | Process Failed");
            }
        }

        private void KillAllRunningSteps()
        {
            foreach (var stepRunnerEntry in _stepRunners)
            {
                stepRunnerEntry.Value.Kill();
            }
        }

        private void WaitAllTasks()
        {
            var tasks = _runningTasks.Values.ToArray();
            // ReSharper disable once CoVariantArrayConversion
            Task.WaitAll(tasks);
        }

        private void WaitAnyTasks()
        {
            var tasks = _runningTasks.Values.ToArray();
            // ReSharper disable once CoVariantArrayConversion
            Task.WaitAny(tasks);
        }

        private void ParseSteps()
        {
            var finished = new List<string>();
            foreach (var taskEntry in _runningTasks)
            {
                var stepName = taskEntry.Key;
                var task = taskEntry.Value;

                if (!task.IsCompleted) continue;
                if (!task.Result) throw new StepFailedException();
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
            var runner = new StepRunner(stepName, step, _actions);
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
            if (step.Prerequisites == null) return true;

            foreach (var prereqStep in step.Prerequisites)
            {
                if (!_finishSteps.Contains(prereqStep)) return false;
            }

            return true;
        }
    }

    public class StepFailedException : Exception
    {
    }
}