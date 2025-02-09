using HardwareDetection;
using User;

namespace OllamaInstaller
{
    class Program
    {
        private const string DownloadUrl = "https://ollama.com/download/OllamaSetup.exe";
        private static readonly string InstallerPath = Path.Combine(Path.GetTempPath(), "OllamaSetup.exe");
        private const string ServiceUrl = "http://localhost:11434";

        static async Task Main(string[] args)
        {
            ConsoleHelper.DrawnLogo();
            if (!OperatingSystem.IsWindows())
            {
                ConsoleHelper.WriteRed("[Status] This application can only run on Windows.");
                return;
            }
            ConsoleHelper.WriteYellow("[SEARCH] Search for Graphics processing unit...");
            if (!DisplayAdapterDetector.DetectHardware()){
                ConsoleHelper.WriteYellow("[INFO] Process terminated...");
                ConsoleHelper.WriteYellow("[ERROR] No dedicated Graphics processing unit can be defined!");
                await User.UserCommands.HandleUserCommandsAsync();
            }
            ConsoleHelper.WriteGreen("[OK] GPU detected!");
            ConsoleHelper.WriteYellow("[SEARCH] Checking if Ollama is installed...");
            bool isInstalled = await OllamaInstallerChecker.IsInstalled();

            if (!isInstalled)
            {
                ConsoleHelper.WriteYellow("[INFO] Downloading installer...");
                await OllamaDownloader.DownloadFile(DownloadUrl, InstallerPath);
                await OllamaInstaller.Install(InstallerPath);
            }
            await OllamaServiceManager.WaitForService(ServiceUrl, isInstalled);
            ConsoleHelper.WriteGreen("[OK] Ollama is ready!");
            ConsoleHelper.WriteYellow("[INFO] Checking models!");
            await InstallModels();
            ConsoleHelper.WriteGreen("[OK] Models are ready!");
            await SocketManager.SocketConnectionManager.StartSocketClient();
            
            await User.UserCommands.HandleUserCommandsAsync(true);
        }
        public static async Task InstallModels()
        {
            await OllamaManager.Manager.PullModelAsync("deepseek-r1:7b");
            ConsoleHelper.WriteGreen("[OK] Deepseek is ready!");
            await OllamaManager.Manager.PullModelAsync("llama3.2");
            ConsoleHelper.WriteGreen("[OK] Llama3 is ready!");
            await OllamaManager.Manager.ListModelsAsync();
        }
    }
}
