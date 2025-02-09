using OllamaInstaller;
using System.Diagnostics;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OllamaSharp.Models.Chat;
using OllamaSharp.Models;

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
        
        public static async Task AllocateWork(string message, bool includeMe = true, string model="deepseek-r1:7b")
        {
            if(model.Length < 3)
            {
                model = "deepseek-r1:7b";
            }
            if (socket != null)
            {
                await socket.EmitAsync("allocateWork", new[] { JsonConvert.SerializeObject(new { message, includeMe, model }) });
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
                socket.On("error", (callback) =>
                {
                    ConsoleHelper.WriteRed($"[DEBUG] ERROR received: {callback}");
                });
                socket.On("completeWork", (response) =>
                {
                    try
                    {
                        string jsonData = response.ToString(); // Biztosítjuk, hogy stringként kezeljük

                        if (!string.IsNullOrWhiteSpace(jsonData))
                        {
                            var data = JsonConvert.DeserializeObject<dynamic>(jsonData);

                            if (data is JArray array && array.Count > 0)
                            {
                                string message = array[0]?.ToString();

                                if (!string.IsNullOrEmpty(message))
                                {
                                    ConsoleHelper.WriteGreen("[RECIVED] " + message);
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
                socket.On("workAllocation", async (response) =>
                {
                    try
                    {
                        string jsonData = response.ToString(); // Biztosítjuk, hogy stringként kezeljük

                        if (!string.IsNullOrWhiteSpace(jsonData))
                        {
                            var data = JsonConvert.DeserializeObject<dynamic>(jsonData);

                            if (data is JArray array && array.Count > 0)
                            {
                                string model = array[0]["model"]?.ToString();
                                string inputMessage = array[0]["message"]?.ToString();
                                string author = array[0]["author"]?.ToString();

                                ConsoleHelper.WriteYellow("[MODEL] " + model);
                                string MaskString(string input)
                                {
                                    if (input.Length <= 4)
                                        return input; 

                                    return input.Substring(0, 4) + new string('*', input.Length - 4);
                                }

                                ConsoleHelper.WriteYellow("[INPUT] " + MaskString(inputMessage));

                                ConsoleHelper.WriteYellow("[AUITHOR] " + author);

                                if (!string.IsNullOrEmpty(model) && !string.IsNullOrEmpty(inputMessage))
                                {
                                    string message = await OllamaManager.Manager.GenerateResponse(model, inputMessage);
                                    await socket.EmitAsync("completeWork", new[] { JsonConvert.SerializeObject(new { message, author }) });
                                    ConsoleHelper.WriteGreen("[INFO] The request delivered to the user.");
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
