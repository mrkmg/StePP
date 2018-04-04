using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using CSharpx;
using Action = StePP.Config.Action;

namespace StePP.Runner
{
    public class ActionRunner
    {
        private readonly Action _action;
        private readonly OutputLogger _outStream;
        private Process _process;
        private TaskCompletionSource<bool> _task;

        public ActionRunner(Action action, OutputLogger outStream)
        {
            _action = action;
            _outStream = outStream;
            CreateTask();
            CreateProcess();
        }

        public async Task<bool> Run()
        {
            if (!_process.Start())
            {
                _task.SetResult(false);
                _process.Dispose();
            }
            else
            {
                _process.BeginErrorReadLine();
                _process.BeginOutputReadLine();
            }

            return await _task.Task;
        }

        public void Kill()
        {
            if (_action.CanBeKilled) _process?.Kill();
        }

        private void CreateTask() => _task = new TaskCompletionSource<bool>();

        private void CreateProcess()
        {
            _process = new Process
            {
                StartInfo =
                {
                    FileName = _action.Executable,
                    Arguments = EncodeArguments(_action.Arguments),
                    CreateNoWindow = true,
                    RedirectStandardError = true,
                    RedirectStandardOutput = true,
                    UseShellExecute = false
                },
                EnableRaisingEvents = true
            };

            if (_action.Environment != null)
                foreach (var envEntry in _action.Environment)
                    _process.StartInfo.EnvironmentVariables.Add(envEntry.Key, envEntry.Value);

            _process.OutputDataReceived += WriteOutput;
            _process.ErrorDataReceived += WriteOutput;
            _process.Exited += TaskExited;
        }

        private void TaskExited(object sender, EventArgs args)
        {
            var process = (Process) sender;
            _task.SetResult(process.ExitCode == 0);
            process.CancelOutputRead();
            process.CancelErrorRead();
            process.Dispose();
        }

        private void WriteOutput(object sender, DataReceivedEventArgs args)
        {
            if (!_outStream.CanWrite || string.IsNullOrEmpty(args.Data)) return;

            var lines = args.Data.Split("\n");

            foreach (var line in lines) _outStream.WriteLine(line);
        }

        private static string EncodeArguments(IEnumerable<string> arguments) => arguments
            .Select(EncodeArgument)
            .ToArray()
            .ToDelimitedString(" ");

        private static string EncodeArgument(string original)
        {
            if (string.IsNullOrEmpty(original))
                return original;
            var value = Regex.Replace(original, @"(\\*)" + "\"", @"$1\$0");
            value = Regex.Replace(value, @"^(.*\s.*?)(\\*)$", "\"$1$2$2\"", RegexOptions.Singleline);

            return value;
        }
    }
}
