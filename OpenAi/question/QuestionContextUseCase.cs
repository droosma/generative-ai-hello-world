using System.Text;

using Azure.AI.OpenAI;

using OpenAi.Ingest;

namespace OpenAi.question;

public class QuestionContextUseCase(
    OpenAIClient openAiClient,
    Database database)
{
    public const string ContextMarker = "----";

    public async Task<(string, Reference[])> Execute(string question)
    {
        /////
        // Create Embedding from question
        /////
        var questionEmbeddingResult = await openAiClient.GetEmbeddingsAsync(new EmbeddingsOptions("text-embedding-ada-002",
                                                                                                  new List<string> {question}));
        var questionEmbedding = questionEmbeddingResult.Value.Data[0].Embedding;

        /////
        // Find top 10 matches based on embedding
        /////
        var questionMatches = await database.Find(questionEmbedding, 10);
        var matches = questionMatches.ToArray();

        /////
        // Build and return prompt
        /////
        var promptBuilder = new StringBuilder();
        promptBuilder.AppendLine(ContextMarker);

        for(var i = 0;i < matches.Length;i++)
        {
            promptBuilder.AppendLine($"MATCH: {matches[i].Content.Optimize()}");
            promptBuilder.AppendLine($"REF: {i}");
        }

        promptBuilder.AppendLine(ContextMarker);

        return (promptBuilder.ToString().Optimize(),
                matches.Select(m => m.Reference).ToArray());
    }
}