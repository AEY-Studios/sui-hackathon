using OllamaInstaller;
using System.Diagnostics;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace SocketManager
{
    public static class SocketConnectionManager
    {
        private const string SocketIoUrl = "https://sui-hackaton-server-q7utg.ondigitalocean.app";
        //private const string SocketIoUrl = "http://localhost:3000";
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
        
        public static async Task AllocateWork(string message)
        {
            if (socket != null)
            {
                await socket.EmitAsync("allocateWork", message);
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
                socket.On("workAllocation", async (response) =>
                {
                    ConsoleHelper.WriteYellow($"[DEBUG] Work Allocation Event received: {response}");

                    try
                    {
                        string jsonData = response.ToString(); // Biztosítjuk, hogy stringként kezeljük

                        if (!string.IsNullOrWhiteSpace(jsonData))
                        {
                            Console.WriteLine(jsonData);
                            var data = JsonConvert.DeserializeObject<dynamic>(jsonData);

                            if (data is JArray array && array.Count > 0)
                            {
                                string model = array[0]["model"]?.ToString();
                                string message = array[0]["message"]?.ToString();

                                if (!string.IsNullOrEmpty(model) && !string.IsNullOrEmpty(message))
                                {
                                    await OllamaManager.Manager.GenerateResponse(model, message);
                                }
                                else
                                {
                                    ConsoleHelper.WriteRed("[ERROR] Invalid JSON structure: Missing 'model' or 'message'.");
                                }
                            }
                            else
                            {
                                ConsoleHelper.WriteRed("[ERROR] JSON is not an array or is empty.");
                            }
                        }
                        else
                        {
                            ConsoleHelper.WriteRed("[ERROR] Received empty JSON data.");
                        }
                    }
                    catch (Exception ex)
                    {
                        ConsoleHelper.WriteRed($"[ERROR] Exception in workAllocation handler: {ex.Message}");
                    }
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
                    if (stopwatch.ElapsedMilliseconds > 500)
                        ConsoleHelper.WriteYellow($"[WARNING] server ping is high: {stopwatch.ElapsedMilliseconds} ms");
                };

                socket.ConnectAsync();
            }
            catch (Exception ex)
            {
                ConsoleHelper.WriteRed($"[ERROR] Error connecting to Socket.IO: {ex.Message}");
                Environment.Exit(0);
            }
        }
    }
}
