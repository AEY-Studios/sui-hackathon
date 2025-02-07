using OllamaInstaller;

namespace User
{
    // Parancs reprezentációja
    public class Command
    {
        public string Name { get; }
        public string Description { get; }
        public Func<string[], Task> Action { get; }

        public Command(string name, string description, Func<string[], Task> action)
        {
            Name = name;
            Description = description;
            Action = action;
        }
    }

    // Segédosztály a spinner futtatásához
    public static class SpinnerHelper
    {
        public static async Task ExecuteWithSpinner(Func<Task> action)
        {
            using (var cts = new CancellationTokenSource())
            {
                // A spinner futtatása egy külön feladaton
                var spinnerTask = Task.Run(() => ShowSpinner(cts.Token), cts.Token);
                try
                {
                    await action();
                }
                finally
                {
                    cts.Cancel();
                    await spinnerTask;
                }
            }
        }

        private static async Task ShowSpinner(CancellationToken token)
        {
            char[] spinnerChars = new[] { '|', '/', '-', '\\' };
            int counter = 0;
            // Kezdetben egy üres karakter, hogy tudjuk majd felülírni
            Console.Write(" ");
            Console.SetCursorPosition(Console.CursorLeft - 1, Console.CursorTop);
            while (!token.IsCancellationRequested)
            {
                Console.Write(spinnerChars[counter]);
                // Visszalépés egy pozícióval, hogy a következő karakter írja felül
                Console.SetCursorPosition(Console.CursorLeft - 1, Console.CursorTop);
                counter = (counter + 1) % spinnerChars.Length;
                try
                {
                    await Task.Delay(100, token);
                }
                catch (TaskCanceledException)
                {
                    break;
                }
            }
            // A spinner eltüntetése a végén
            Console.Write(" ");
            Console.SetCursorPosition(Console.CursorLeft - 1, Console.CursorTop);
        }
    }

    public static class UserCommands
    {
        // A parancsokat egy dictionary-ben tároljuk, hogy könnyen bővíthető és karbantartható legyen.
        private static readonly Dictionary<string, Command> Commands = new(StringComparer.OrdinalIgnoreCase)
        {
            {
                "exit",
                new Command(
                    "exit",
                    "Stops the application",
                    async args =>
                    {
                        ConsoleHelper.WriteYellow("[EXIT] Stopping application...");
                        await SocketManager.SocketConnectionManager.StopSocketClient();
                        Environment.Exit(0);
                    }
                )
            },
            {
                "status",
                new Command(
                    "status",
                    "Prints the current socket status",
                    async args =>
                    {
                        ConsoleHelper.WriteGreen($"[Status] {SocketManager.SocketConnectionManager.GetSocketStatus()}");
                        await Task.CompletedTask;
                    }
                )
            },
            {
                "workers",
                new Command(
                    "workers",
                    "Lists all workers",
                    async args =>
                    {
                        await SpinnerHelper.ExecuteWithSpinner(async () =>
                        {
                            await SocketManager.SocketConnectionManager.ListWorkers();
                        });
                    }
                )
            },
            {
                "models",
                new Command(
                    "models",
                    "Lists available models",
                    async args =>
                    {
                        await SpinnerHelper.ExecuteWithSpinner(async () =>
                        {
                            await OllamaManager.Manager.ListModelsAsync();
                        });
                    }
                )
            },
            {
                "test",
                new Command(
                    "test",
                    "run a local test | args: model message",
                    async args =>
                    {
                        await SpinnerHelper.ExecuteWithSpinner(async () =>
                        {
                            if (args.Length < 2)
                            {
                                ConsoleHelper.WriteRed("[Error] The 'test' command requires two arguments: model and message.");
                            }
                            else
                            {
                                string message = string.Join(" ", args[1..]);
                                await OllamaManager.Manager.LocalTest(args[0], message);
                            }
                        });
                    }
                )
            },
            {
                "pull",
                new Command(
                    "pull",
                    "pull model | args: model",
                    async args =>
                    {
                        await SpinnerHelper.ExecuteWithSpinner(async () =>
                        {
                            if (args.Length < 1)
                            {
                                ConsoleHelper.WriteRed("[Error] The 'test' command requires two arguments: model and message.");
                            }
                            else
                            {
                                await OllamaManager.Manager.PullModelAsync(args[0]);
                            }
                        });
                    }
                )
            },
            {
                "help",
                new Command(
                    "help",
                    "Lists all available commands",
                    async args =>
                    {
                        ConsoleHelper.WriteGreen("Available commands:");
                        foreach (var cmd in Commands.Values)
                        {
                            ConsoleHelper.WriteGreen($"- {cmd.Name}: {cmd.Description}");
                        }
                        await Task.CompletedTask;
                    }
                )
            },
        };

        public static async Task HandleUserCommands()
        {
            while (true)
            {
                Console.Write("> ");
                var inputLine = Console.ReadLine()?.Trim();
                if (string.IsNullOrWhiteSpace(inputLine))
                    continue;

                // Egyszerű paraméterfeldolgozás: az első szó a parancs, a többi pedig paraméterek
                var parts = inputLine.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                var commandName = parts[0].ToLower();
                var args = parts.Length > 1 ? parts[1..] : Array.Empty<string>();

                if (Commands.TryGetValue(commandName, out var command))
                {
                    await command.Action(args);
                }
                else
                {
                    ConsoleHelper.WriteRed("[Status] Unknown command. Type 'help' for a list of available commands.");
                }
            }
        }
    }
}
