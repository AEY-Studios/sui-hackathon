using OllamaInstaller;
using System.Diagnostics;

namespace SocketManager
{
    public static class SocketConnectionManager
    {
        private const string SocketIoUrl = "http://localhost:3000";
        private static SocketIOClient.SocketIO? socket;

        public static async Task StartSocketClient()
        {
            ConsoleHelper.WriteYellow("[INFO] Starting Socket.IO client...");
            await StartSocketIoClient();
        }

        public static async Task StopSocketClient()
        {
            if (socket != null)
            {
                await socket.DisconnectAsync();
                ConsoleHelper.WriteRed("[Status] Socket.IO client stopped.");
            }
        }

        public static async Task ListWorkers()
        {
            if (socket != null)
            {
                await socket.EmitAsync("getWorkers");
            }
        }

        public static string GetSocketStatus()
        {
            if (socket == null) return "[STATUS] Not initialized";
            return socket.Connected ? "[STATUS] Connected" : "[STATUS] Disconnected";
        }

        private static async Task StartSocketIoClient()
        {
            try
            {
                socket = new SocketIOClient.SocketIO(SocketIoUrl, new SocketIOClient.SocketIOOptions
                {
                    Reconnection = true,
                    ReconnectionAttempts = 10
                });

                socket.OnConnected += (sender, e) =>
                {
                    ConsoleHelper.WriteGreen("[OK] Connected to Socket.IO server.");
                };
                socket.On("workerList", (callback) =>
                {
                    ConsoleHelper.WriteYellow($"[DEBUG] Event received: {callback}");
                });
                socket.OnDisconnected += (sender, e) =>
                {
                    ConsoleHelper.WriteRed("[EVENT] Disconnected from Socket.IO server.");
                };
                socket.OnError += (sender, e) =>
                {
                    ConsoleHelper.WriteRed("[ERROR] " + e);
                };
                socket.OnReconnectAttempt += (sender, e) =>
                {
                    ConsoleHelper.WriteYellow("[STATUS] Reconnect attempt");
                };
                socket.OnReconnected += (sender, e) =>
                {
                    ConsoleHelper.WriteGreen("[STATUS] Reconnected");
                };
                Stopwatch stopwatch = new Stopwatch();
                socket.OnPing += (sender, e) =>
                {
                    stopwatch.Restart();
                };

                socket.OnPong += (sender, e) =>
                {
                    stopwatch.Stop();
                    ConsoleHelper.WriteGreen($"[STATUS] server ping: {stopwatch.ElapsedMilliseconds} ms");
                };

                socket.ConnectAsync();
                ConsoleHelper.WriteGreen("[EVENT] Listening for messages...");
            }
            catch (Exception ex)
            {
                ConsoleHelper.WriteRed($"[ERROR] Error connecting to Socket.IO: {ex.Message}");
                Environment.Exit(0);
            }
        }
    }
}
