using OllamaInstaller;
using OllamaSharp;
using System.Text.RegularExpressions;

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

        public static async Task<string> GenerateResponse(string model, string msg)
        {
            var generateRequest = new OllamaSharp.Models.GenerateRequest();
            generateRequest.Model = model;
            generateRequest.Prompt = msg;
            generateRequest.Stream = false;
            try
            {
                await foreach (var response in Ollama.GenerateAsync(generateRequest))
                {
                    ConsoleHelper.WriteGreen("[DONE] One request has been answered. | Input token: " + EstimateTokens(msg, model) + " | Response token: " + EstimateTokens(response.Response, model));
                    ConsoleHelper.WriteGreen("[EARNED] Gained reward: " + (EstimateTokens(msg, model) + EstimateTokens(response.Response, model)) * 0.025f);
                    return response.Response;
                }
                return "";
            }
            catch (Exception e)
            {
                ConsoleHelper.WriteRed("[ERROR] " + e.Message);
                return e.Message;
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

        public static int EstimateTokens(string text, string modelType)
        {
            if (string.IsNullOrWhiteSpace(text))
                return 0;

            string[] words = Regex.Split(text, @"\s+|(?=[.,!?;])|(?<=[.,!?;])");

            if (modelType.Contains("llama"))
            {
                return (int)(words.Length * 1.2);
            }
            else if (modelType.Contains("deepseek"))
            {
                return (int)(words.Length * 1.15);
            }
            else
            {
                return (int)(words.Length * 3);
            }
        }
    }
}
