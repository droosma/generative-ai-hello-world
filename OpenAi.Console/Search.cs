using Azure.AI.OpenAI;

using Microsoft.Extensions.Configuration;

using NRedisStack;
using NRedisStack.RedisStackCommands;
using NRedisStack.Search;

namespace OpenAi.Console;

public static class Search
{
    private const string SystemPrompt = @"
            The following is a conversation with an AI specialist doctor assistant. 
            The assistant is helpful, creative, clever, and very friendly with the task to assist other doctors find information about medical guidelines.
            Your answer must abide by the following rules:
                 1: Your answer must be based solely on the provided sources. 
                 2: Every part of the answer must be supported only by the sources.
                 3: If the answer consists of steps, provide a clear bullet point list.
                 4: If you don't know the answer, just say that you don't know. Don't try to make up an answer.
                 5: Be concise and provide one final answer.
                 6: NEVER provide questions in the answer.

            Human: What is the pharmaceutical shape of Enbrel?
            AI: The pharmaceutical shape of Enbrel is a powder for solution for injection.
    ";

    private const string QuestionPromptTemplate = @"
        Sources:++++++{0}++++++
        Question: {1}?
        Answer:
    ";

    public static async Task Execute(IConfiguration config)
    {
        var openAiSettings = config.GetSection("OpenAI")
                                   .Get<OpenAISettings>() ?? throw new ArgumentException("OpenAI configuration failed");
        var openAiClient = openAiSettings.Client;
        System.Console.WriteLine("Enter search term:");
        var query = System.Console.ReadLine();

        var questionEmbeddingResult = await openAiClient.GetEmbeddingsAsync(openAiSettings.EmbeddingModelId, new EmbeddingsOptions(query));
        var questionEmbedding = questionEmbeddingResult.Value.Data[0].Embedding;

        var redisSettings = config.GetSection("RedisPersistence")
                                  .Get<RedisSettings>() ?? throw new ArgumentException("RedisPersistence configuration failed");
        var databaseClient = redisSettings.Database;

        ISearchCommands searchCommands = databaseClient.FT();
        var searchResult = searchCommands.Search(redisSettings.Index,
                                                 new Query("*=>[KNN $top @embedding $embedding AS score]")
                                                     .AddParam("embedding", questionEmbedding.AsBytes())
                                                     .AddParam("top", 3)
                                                     .ReturnFields("content")
                                                     .Dialect(2));
        var sources = string.Join(" ", searchResult.Documents.Select(document => document["content"]));

        var response = await openAiClient.GetChatCompletionsAsync(openAiSettings.ChatModelId, new ChatCompletionsOptions
                                                                                              {
                                                                                                  Temperature = 0,
                                                                                                  Messages =
                                                                                                  {
                                                                                                      new ChatMessage(ChatRole.System, SystemPrompt),
                                                                                                      new ChatMessage(ChatRole.User, string.Format(QuestionPromptTemplate, sources, query))
                                                                                                  }
                                                                                              });
        System.Console.WriteLine(response.Value.Choices[0].Message.Content);
        System.Console.ReadLine();
    }
}