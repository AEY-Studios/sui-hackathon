using System.Diagnostics;

namespace OllamaInstaller
{
    public static class OllamaInstaller
    {
        public static async Task Install(string installerPath)
        {
            var psi = new ProcessStartInfo
            {
                FileName = "powershell",
                Arguments = $"-Command \"Start-Process -FilePath '{installerPath}' -ArgumentList '/SILENT' -Wait\"",
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using (var process = Process.Start(psi))
            {
                await process.WaitForExitAsync();
                if (process.ExitCode != 0)
                {
                    throw new Exception("Installation process failed.");
                }
            }
        }
    }
}
