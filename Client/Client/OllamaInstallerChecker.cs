using System.Diagnostics;

namespace OllamaInstaller
{
    public static class OllamaInstallerChecker
    {
        public static async Task<bool> IsInstalled()
        {
            try
            {
                var psi = new ProcessStartInfo
                {
                    FileName = "powershell",
                    Arguments = "-Command \"Get-Package -Name '*ollama*' -ErrorAction SilentlyContinue | Out-String\"",
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                using (var process = Process.Start(psi))
                {
                    string output = await process.StandardOutput.ReadToEndAsync();
                    await process.WaitForExitAsync();

                    if (process.ExitCode != 0)
                        return false;

                    return output.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries).Length > 2;
                }
            }
            catch
            {
                return false;
            }
        }

        public static async Task<bool> IsServiceRunning(string url, int timeoutSeconds = 5)
        {
            using (var httpClient = new HttpClient())
            {
                httpClient.Timeout = TimeSpan.FromSeconds(timeoutSeconds);
                try
                {
                    var response = await httpClient.GetAsync(url);
                    return response.IsSuccessStatusCode;
                }
                catch
                {
                    return false;
                }
            }
        }
    }
}
