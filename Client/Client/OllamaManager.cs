using OllamaSharp;

namespace OllamaManager
{
    public static class Manager
    {
        private static readonly Uri Uri = new Uri("http://localhost:11434");
        private static readonly OllamaApiClient Ollama = new OllamaApiClient(Uri);

        public static async Task ListModelsAsync()
        {
            var models = await Ollama.ListLocalModelsAsync();
            Console.WriteLine("Available models:");
            foreach (var model in models)
            {
                Console.WriteLine($"- {model.Name} - {model.Size}");
            }
        }

        public static async Task PullModelAsync(string model)
        {
            try
            {
                await foreach (var status in Ollama.PullModelAsync(model))
                {
                    Console.SetCursorPosition(0, Console.CursorTop);
                    Console.Write($"{status.Percent}% {status.Status}");
                }
                Console.WriteLine();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        public static async Task LocalTest(string model, string msg)
        {
            Ollama.SelectedModel = model;
            try
            {
                await foreach (var stream in Ollama.GenerateAsync(msg))
                {
                    Console.Write(stream.Response);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }
    }
}
