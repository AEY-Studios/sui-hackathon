using System.Diagnostics;

namespace OllamaInstaller
{
    public static class OllamaServiceManager
    {
        public static async Task StartService(string ollamaPath)
        {
            var psi = new ProcessStartInfo
            {
                FileName = ollamaPath,
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

        public static async Task WaitForService(string url)
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
                        // Ignore exceptions and continue retrying.
                    }

                    if (!warned && (DateTime.Now - startTime).TotalSeconds >= 30)
                    {
                        ConsoleHelper.WriteYellow("Service is taking longer than usual...");
                        warned = true;
                    }
                    await Task.Delay(1000);
                }
            }
        }
    }
}
