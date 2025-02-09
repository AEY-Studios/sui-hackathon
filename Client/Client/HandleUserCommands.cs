using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using CommandLine;
using OllamaInstaller;

namespace User
{
    [Verb("exit", HelpText = "Stops the application.")]
    public class ExitOptions { }

    [Verb("status", HelpText = "Prints the current socket status.")]
    public class StatusOptions { }

    [Verb("workers", HelpText = "Lists all workers.")]
    public class WorkersOptions { }

    [Verb("models", HelpText = "Lists available models.")]
    public class ModelsOptions { }

    [Verb("test", HelpText = "Runs a local test. Arguments: --model <model> --message <message>")]
    public class TestOptions
    {
        [Option("model", Required = true, HelpText = "The model name.")]
        public string Model { get; set; }

        [Option("message", Required = true, HelpText = "The message to send.", Min = 1)]
        public IEnumerable<string> MessageParts { get; set; }
    }

    [Verb("pull", HelpText = "Pulls a model. Argument: --model <model>")]
    public class PullOptions
    {
        [Option("model", Required = true, HelpText = "The model name.")]
        public string Model { get; set; }
    }

    [Verb("allocate", HelpText = "Allocates work to a worker. Argument: --message <message> --includeme bool")]
    public class AllocateOptions
    {
        [Option("message", Required = true, HelpText = "The work message.", Min = 1)]
        public IEnumerable<string> MessageParts { get; set; }

        [Option("includeme", Required = false, HelpText = "Allow assignment to my machine (default true)")]
        public bool? IncludeMe { get; set; }
    }

    [Verb("help", HelpText = "Lists all available commands.")]
    public class HelpOptions { }

    public static class SpinnerHelper
    {
        public static async Task ExecuteWithSpinner(Func<Task> action)
        {
            using (var cts = new CancellationTokenSource())
            {
                var spinnerTask = Task.Run(() => ShowSpinner(cts.Token), cts.Token);
                try { await action(); }
                finally { cts.Cancel(); await spinnerTask; }
            }
        }

        private static async Task ShowSpinner(CancellationToken token)
        {
            char[] spinnerChars = new[] { '|', '/', '-', '\\' };
            int counter = 0;
            Console.Write(" ");
            Console.SetCursorPosition(Console.CursorLeft - 1, Console.CursorTop);
            while (!token.IsCancellationRequested)
            {
                Console.Write(spinnerChars[counter]);
                Console.SetCursorPosition(Console.CursorLeft - 1, Console.CursorTop);
                counter = (counter + 1) % spinnerChars.Length;
                try { await Task.Delay(100, token); }
                catch (TaskCanceledException) { break; }
            }
            Console.Write(" ");
            Console.SetCursorPosition(Console.CursorLeft - 1, Console.CursorTop);
        }
    }

    public static class UserCommands
    {
        private static string[] SplitArguments(string commandLine)
        {
            var pattern = @"(?<=^|\s)(?:""(?<arg>[^""]+)""|(?<arg>\S+))";
            var matches = Regex.Matches(commandLine, pattern);
            return matches.Cast<Match>().Select(m => m.Groups["arg"].Value).ToArray();
        }

        public static async Task HandleUserCommandsAsync()
        {
            while (true)
            {
                Console.Write("> ");
                var inputLine = Console.ReadLine()?.Trim();
                if (string.IsNullOrWhiteSpace(inputLine))
                    continue;
                var args = SplitArguments(inputLine);
                await Parser.Default.ParseArguments<
                    ExitOptions,
                    StatusOptions,
                    WorkersOptions,
                    ModelsOptions,
                    TestOptions,
                    PullOptions,
                    AllocateOptions,
                    HelpOptions>(args)
                .MapResult(
                    async (ExitOptions opts) => await RunExitCommand(opts),
                    async (StatusOptions opts) => await RunStatusCommand(opts),
                    async (WorkersOptions opts) => await RunWorkersCommand(opts),
                    async (ModelsOptions opts) => await RunModelsCommand(opts),
                    async (TestOptions opts) => await RunTestCommand(opts),
                    async (PullOptions opts) => await RunPullCommand(opts),
                    async (AllocateOptions opts) => await RunAllocateCommand(opts),
                    async (HelpOptions opts) => await RunHelpCommand(opts),
                    errs => Task.CompletedTask
                );
            }
        }

        private static async Task RunExitCommand(ExitOptions opts)
        {
            ConsoleHelper.WriteYellow("[EXIT] Stopping application...");
            await SocketManager.SocketConnectionManager.StopSocketClient();
            Environment.Exit(0);
        }

        private static async Task RunStatusCommand(StatusOptions opts)
        {
            ConsoleHelper.WriteGreen($"[Status] {SocketManager.SocketConnectionManager.GetSocketStatus()}");
            await Task.CompletedTask;
        }

        private static async Task RunWorkersCommand(WorkersOptions opts)
        {
            await SpinnerHelper.ExecuteWithSpinner(async () =>
            {
                await SocketManager.SocketConnectionManager.ListWorkers();
            });
        }

        private static async Task RunModelsCommand(ModelsOptions opts)
        {
            await SpinnerHelper.ExecuteWithSpinner(async () =>
            {
                await OllamaManager.Manager.ListModelsAsync();
            });
        }

        private static async Task RunTestCommand(TestOptions opts)
        {
            await SpinnerHelper.ExecuteWithSpinner(async () =>
            {
                string message = string.Join(" ", opts.MessageParts);
                await OllamaManager.Manager.LocalTest(opts.Model, message);
            });
        }

        private static async Task RunPullCommand(PullOptions opts)
        {
            await SpinnerHelper.ExecuteWithSpinner(async () =>
            {
                await OllamaManager.Manager.PullModelAsync(opts.Model);
            });
        }

        private static async Task RunAllocateCommand(AllocateOptions opts)
        {
            await SpinnerHelper.ExecuteWithSpinner(async () =>
            {
                string message = string.Join(" ", opts.MessageParts);
                bool includeMe = opts.IncludeMe ?? true; 

                await SocketManager.SocketConnectionManager.AllocateWork(message, includeMe);
            });
        }

        private static async Task RunHelpCommand(HelpOptions opts)
        {
            ConsoleHelper.WriteGreen("Available commands:");
            ConsoleHelper.WriteGreen("- exit: Stops the application.");
            ConsoleHelper.WriteGreen("- status: Prints the current socket status.");
            ConsoleHelper.WriteGreen("- workers: Lists all workers.");
            ConsoleHelper.WriteGreen("- models: Lists available models.");
            ConsoleHelper.WriteGreen("- test: Runs a local test. Arguments: --model <model> --message <message>.");
            ConsoleHelper.WriteGreen("- pull: Pulls a model. Argument: --model <model>.");
            ConsoleHelper.WriteGreen("- allocate: Allocates work to a worker. Argument: --message <message>.");
            ConsoleHelper.WriteGreen("- help: Lists all available commands.");
            await Task.CompletedTask;
        }
    }
}
