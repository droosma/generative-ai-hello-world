using Azure;
using Azure.AI.FormRecognizer.DocumentAnalysis;
using Azure.AI.OpenAI;

using StackExchange.Redis;

namespace OpenAi.Console;

public record OpenAISettings(string Key,
                             string? EndPoint,
                             string EmbeddingModelId = "text-embedding-ada-002",
                             string ChatModelId = "gpt-3.5-turbo")
{
    public OpenAIClient Client => string.IsNullOrWhiteSpace(EndPoint)
                                      ? new OpenAIClient(Key)
                                      : new OpenAIClient(new Uri(EndPoint), new AzureKeyCredential(Key));
}

public record RedisSettings(string ConnectionString,
                            string Index,
                            int Db = 0)
{
    public IDatabase Database => ConnectionMultiplexer.Connect(ConnectionString).GetDatabase(Db);
}

public record FormRecognizerSettings(string Key, string EndPoint)
{
    public DocumentAnalysisClient DocumentAnalysisClient => new(new Uri(EndPoint), new AzureKeyCredential(Key));
}