using Azure;
using Azure.AI.FormRecognizer.DocumentAnalysis;
using Azure.AI.OpenAI;

using Microsoft.Extensions.Configuration;

namespace OpenAi.Console;

public record OpenAISettings(string Key, string EndPoint)
{
    public OpenAIClient Client => string.IsNullOrWhiteSpace(EndPoint)
                                      ? new OpenAIClient(Key)
                                      : new OpenAIClient(new Uri(EndPoint), new AzureKeyCredential(Key));

    public static OpenAISettings From(IConfigurationSection section)
    {
        var key = section.GetValue<string>("Key");
        var endPoint = section.GetValue<string>("EndPoint");
        return new OpenAISettings(key!, endPoint!);
    }
}

public record DocumentAnalysisSettings(string Key, string EndPoint)
{
    public DocumentAnalysisClient DocumentAnalysisClient => new(new Uri(EndPoint), new AzureKeyCredential(Key));

    public static DocumentAnalysisSettings From(IConfigurationSection section)
    {
        var key = section.GetValue<string>("Key");
        var endPoint = section.GetValue<string>("EndPoint");
        return new DocumentAnalysisSettings(key!, endPoint);
    }
}