using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
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
        private Process _currentProcess;

        public ActionRunner(Action action, OutputLogger outStream)
        {
            _action = action;
            _outStream = outStream;
        }

        public async Task<bool> Run()
        {
            var taskCompletion = new TaskCompletionSource<bool>();
            _currentProcess = new Process
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
                {
                    _currentProcess.StartInfo.EnvironmentVariables.Add(envEntry.Key, envEntry.Value);
                }

            _currentProcess.OutputDataReceived += WriteOutput;
            _currentProcess.ErrorDataReceived += WriteOutput;

            _currentProcess.Exited += (sender, args) =>
            {
                var process = (Process) sender;
                taskCompletion.SetResult(process.ExitCode == 0);
                process.CancelOutputRead();
                process.CancelErrorRead();
                process.Dispose();
            };

            if (!_currentProcess.Start())
            {
                taskCompletion.SetResult(false);
                _currentProcess.Dispose();
            }

            _currentProcess.BeginErrorReadLine();
            _currentProcess.BeginOutputReadLine();

            return await taskCompletion.Task;
        }

        public void Kill()
        {
            if (_action.CanBeKilled)
                _currentProcess?.Kill();
        }

        private void WriteOutput(object sender, DataReceivedEventArgs args)
        {
            if (!_outStream.CanWrite || string.IsNullOrEmpty(args.Data)) return;

            var lines = args.Data.Split("\n");

            foreach (var line in lines)
            {
                _outStream.WriteLine(line);
            }
        }

        private static string EncodeArguments(IReadOnlyCollection<string> arguments)
        {
            return arguments
                .Select(EncodeArgument)
                .ToArray()
                .ToDelimitedString(" ");
        }

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
