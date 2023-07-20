using Azure;
using Azure.AI.OpenAI;

using Microsoft.Extensions.Configuration;

using StackExchange.Redis;

namespace OpenAi.Console;

public static class Ingest
{
    public static async Task Execute(IConfiguration config)
    {
        System.Console.WriteLine("Starting Ingestion");

        await using var fileStream = File.OpenRead(@"..\..\..\..\doc\enbrel-epar-product-information_nl.pdf");
        var memStream = new MemoryStream();
        await fileStream.CopyToAsync(memStream);
        memStream.Position = 0;

        var documentAnalysisClient = config.GetSection("CognitiveServices")
                                           .Get<FormRecognizerSettings>()?
                                           .DocumentAnalysisClient ?? throw new ArgumentException("CognitiveServices configuration failed");
        var ingestionResult = await documentAnalysisClient.AnalyzeDocumentAsync(WaitUntil.Completed, "prebuilt-read", memStream);

        System.Console.WriteLine($"Retrieved content from file {ingestionResult.Value.Pages.Count}");

        var chunks = ingestionResult.Value.Pages.Select(page => new
                                                                {
                                                                    Content = string.Join(" ", page.Lines.Select(l => l.Content)),
                                                                    Page = page.PageNumber
                                                                });

        System.Console.WriteLine("Generate Embedding for chunks and persist");

        var redisSettings = config.GetSection("RedisPersistence")
                                  .Get<RedisSettings>() ?? throw new ArgumentException("RedisPersistence configuration failed");
        var databaseClient = redisSettings.Database;
        var openAiSettings = config.GetSection("OpenAI")
                                   .Get<OpenAISettings>() ?? throw new ArgumentException("OpenAI configuration failed");
        var openAiClient = openAiSettings.Client;
        System.Console.WriteLine("Processing chunk:");

        var tasks = chunks.Select(async chunk =>
                                  {
                                      var result = await openAiClient.GetEmbeddingsAsync(openAiSettings.EmbeddingModelId, new EmbeddingsOptions(chunk.Content));
                                      var embedding = result.Value.Data[0].Embedding;
                                      await databaseClient.HashSetAsync($"{redisSettings.Index}:{chunk.Page}",
                                                                        new HashEntry[] {new("content", chunk.Content), new("embedding", embedding.AsBytes())});
                                      System.Console.Write(".");
                                  });

        await Task.WhenAll(tasks);

        System.Console.WriteLine();
        System.Console.WriteLine("Done");
    }
}