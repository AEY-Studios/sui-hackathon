namespace OllamaInstaller
{
    class Program
    {
        private const string DownloadUrl = "https://ollama.com/download/OllamaSetup.exe";
        private static readonly string InstallerPath = Path.Combine(Path.GetTempPath(), "OllamaSetup.exe");
        private const string ServiceUrl = "http://localhost:11434";

        static async Task Main(string[] args)
        {
            if (!OperatingSystem.IsWindows())
            {
                ConsoleHelper.WriteRed("This application can only run on Windows.");
                return;
            }

            ConsoleHelper.WriteYellow("[SEARCH] Checking if Ollama is installed...");
            bool isInstalled = await OllamaInstallerChecker.IsInstalled();

            if (!isInstalled)
            {
                ConsoleHelper.WriteYellow("Downloading installer...");
                await OllamaDownloader.DownloadFile(DownloadUrl, InstallerPath);
                await OllamaInstaller.Install(InstallerPath);
            }

            await OllamaServiceManager.WaitForService(ServiceUrl);
            ConsoleHelper.WriteGreen("Ollama is ready!");
        }
    }
}
