namespace OllamaInstaller
{
    public static class ConsoleHelper
    {
        public static void WriteYellow(string message)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine(message);
            Console.ResetColor();
        }

        public static void WriteGreen(string message)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine(message);
            Console.ResetColor();
        }

        public static void WriteRed(string message)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(message);
            Console.ResetColor();
        }

        public static void DrawTextProgressBar(long progress, long total, int barSize)
        {
            double percentage = (double)progress / total;
            int filledBarSize = (int)(percentage * barSize);
            string bar = new string('=', filledBarSize) + new string(' ', barSize - filledBarSize);
            Console.Write($"\rDownloading [{bar}] {percentage * 100:0.00}%");
        }
    }
}
