using Azure;
using Azure.AI.FormRecognizer.DocumentAnalysis;
using Azure.AI.OpenAI;

using Polly;
using Polly.Retry;

namespace OpenAi.Ingest;

public class IngestionUseCase(
    OpenAIClient openAiClient,
    DocumentAnalysisClient documentAnalysisClient,
    FileSystem fileSystem,
    Database database)
{
    private readonly AsyncRetryPolicy _retryPolicy = Policy.Handle<RequestFailedException>(ex => ex.Status == 429)
                                                           .WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));

    public async Task Execute()
    {
        /////
        Console.WriteLine("get the some input");
        /////
        var stream = await fileSystem.Load("enbrel-epar-product-information_en.pdf");

        /////
        Console.WriteLine("extract text from PDF using Microsoft Azure Forms recognizer");
        /////
        var operation = await documentAnalysisClient.AnalyzeDocumentAsync(WaitUntil.Completed, "prebuilt-read", stream);
        var analyzeResult = operation.Value;

        /////
        Console.WriteLine("Create partitions from the text");
        /////
        var lines = analyzeResult.Pages
                                 .SelectMany(page => page.Lines.Select((line, index) => (line.Content, page.PageNumber, LineNumber:index + 1)))
                                 .ToList();
        const int PartitionSize = 100;
        const int OverlapSize = 5;

        var numberOfPartitions = (int) Math.Ceiling((lines.Count - OverlapSize) / (double) (PartitionSize - OverlapSize));
        var partitions = Enumerable.Range(0, numberOfPartitions)
                               .Select(index =>
                                       {
                                               var startLine = index * (PartitionSize - OverlapSize);
                                           var overlapStart = Math.Max(0, startLine - OverlapSize);
                                               var endLine = Math.Min(overlapStart + PartitionSize, lines.Count);
                                               var partitionLines = lines.GetRange(overlapStart, endLine - overlapStart);
                                               return Partition.From(partitionLines);
                                       })
                               .ToList();
        Console.WriteLine($"Created {partitions.Count} partitions from {lines.Count} found Lines");

        /////
        Console.WriteLine("Create embeddings from the partitions");
        /////
        Console.WriteLine();
        var embeddingTasks = partitions.Select(async chunk =>
           await _retryPolicy.ExecuteAsync(async () =>
                                           {
                                               var result = await openAiClient.GetEmbeddingsAsync(new EmbeddingsOptions("text-embedding-ada-002",
                                                                                                new List<string> {chunk.EmbeddingContent}));
                                               Console.Write(".");
                                               return Embedding.From(chunk, result.Value.Data[0].Embedding);
                                           }));
        var embeddings = await Task.WhenAll(embeddingTasks);
        Console.WriteLine();

        /////
        Console.WriteLine("Persist the embeddings");
        /////
        await database.Save(embeddings);

        Console.WriteLine("Done");
        Console.ReadLine();
    }
}