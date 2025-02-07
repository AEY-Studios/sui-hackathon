using System.Diagnostics;

namespace OllamaInstaller
{
    public static class OllamaServiceManager
    {
        private static async Task StartOllamaProcess()
        {
            string ollamaPath = GetOllamaPath();

            if (string.IsNullOrEmpty(ollamaPath))
            {
                ConsoleHelper.WriteRed("Ollama executable not found.");
                return;
            }
            ConsoleHelper.WriteGreen("Ollama executable found: " + ollamaPath);
            var psi = new ProcessStartInfo
            {
                FileName = ollamaPath,
                Arguments = "",
                UseShellExecute = false,
                CreateNoWindow = true
            };

            try
            {
                Process.Start(psi);
                await Task.Delay(2000); // Wait for the process to start
            }
            catch (Exception ex)
            {
                ConsoleHelper.WriteRed("Error starting Ollama: " + ex.Message);
            }
        }

        private static string GetOllamaPath()
        {
            try
            {
                var psi = new ProcessStartInfo
                {
                    FileName = "powershell",
                    Arguments = "-Command \"(Get-Package -Name '*ollama*').FastPackageReference\"",
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                using (var process = new Process { StartInfo = psi })
                {
                    process.Start();
                    string output = process.StandardOutput.ReadToEnd().Trim();
                    process.WaitForExit();

                    if (!string.IsNullOrEmpty(output))
                    {
                        // Get the registry path from PowerShell output
                        string uninstallRegPath = output.Replace("hkcu64\\", "HKCU:\\"); // Ensure correct PS format

                        // Additional PowerShell command to query DisplayIcon (unins000.exe)
                        var psi2 = new ProcessStartInfo
                        {
                            FileName = "powershell",
                            Arguments = $"-Command \"(Get-ItemProperty '{uninstallRegPath}').DisplayIcon\"",
                            RedirectStandardOutput = true,
                            UseShellExecute = false,
                            CreateNoWindow = true
                        };

                        using (var process2 = new Process { StartInfo = psi2 })
                        {
                            process2.Start();
                            string uninstallPath = process2.StandardOutput.ReadToEnd().Trim();
                            process2.WaitForExit();

                            if (!string.IsNullOrEmpty(uninstallPath))
                            {
                                // Extract the directory of the uninstall exe
                                string installDir = Path.GetDirectoryName(uninstallPath);
                                string ollamaExePath = Path.Combine(installDir, "ollama app.exe");

                                // Check if ollama.exe exists
                                if (File.Exists(ollamaExePath))
                                {
                                    return ollamaExePath;
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ConsoleHelper.WriteRed("Error finding Ollama path: " + ex.Message);
            }

            return null;
        }

        public static async Task WaitForService(string url, bool isInstalled)
        {
            using (var httpClient = new HttpClient())
            {
                DateTime startTime = DateTime.Now;
                bool warned = false;
                while (true)
                {
                    try
                    {
                        var response = await httpClient.GetAsync(url);
                        if (response.IsSuccessStatusCode)
                        {
                            break;
                        }
                    }
                    catch
                    {
                        if (isInstalled)
                        {
                            ConsoleHelper.WriteYellow("[INFO] Ollama is installed but not running...");
                            await StartOllamaProcess();
                        }
                    }

                    if (!warned && (DateTime.Now - startTime).TotalSeconds >= 30)
                    {
                        ConsoleHelper.WriteYellow("[INFO] Service is taking longer than usual...");
                        warned = true;
                    }
                    await Task.Delay(1000);
                }
            }
        }
    }
}
