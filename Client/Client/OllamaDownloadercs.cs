namespace OllamaInstaller
{
    public static class OllamaDownloader
    {
        public static async Task DownloadFile(string url, string destinationPath)
        {
            using (var httpClient = new HttpClient(new HttpClientHandler { AllowAutoRedirect = true }))
            {
                using (var response = await httpClient.GetAsync(url, HttpCompletionOption.ResponseHeadersRead))
                {
                    response.EnsureSuccessStatusCode();
                    long? totalBytes = response.Content.Headers.ContentLength;
                    using (var contentStream = await response.Content.ReadAsStreamAsync())
                    {
                        await ProcessContentStream(totalBytes, contentStream, destinationPath);
                    }
                }
            }
        }

        private static async Task ProcessContentStream(long? totalDownloadSize, Stream contentStream, string destinationPath)
        {
            const int bufferSize = 81920;
            var buffer = new byte[bufferSize];
            long totalBytesRead = 0;
            int bytesRead;

            using (var fileStream = new FileStream(destinationPath, FileMode.Create, FileAccess.Write, FileShare.None, bufferSize, true))
            {
                while ((bytesRead = await contentStream.ReadAsync(buffer, 0, buffer.Length)) > 0)
                {
                    await fileStream.WriteAsync(buffer, 0, bytesRead);
                    totalBytesRead += bytesRead;
                    if (totalDownloadSize.HasValue)
                    {
                        ConsoleHelper.DrawTextProgressBar(totalBytesRead, totalDownloadSize.Value, 40);
                    }
                }
            }
            Console.WriteLine();
        }
    }
}
