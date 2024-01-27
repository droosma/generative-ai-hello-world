using System.Text.Json;

using Azure;
using Azure.AI.OpenAI;

using OpenAi.question;

namespace OpenAi.Web;

public class ConversationWithReferences(
    QuestionContextUseCase useCase,
    OpenAIClient openAiClient) : IConversation
{
    private const string _systemMessage = $$"""
                                            You are a helpful assistant designed to assist a Consumer with questions about specific documentation. Your task is to answer the user's question accurately using only the context provided between '{{QuestionContextUseCase.ContextMarker}}'. The context includes matches, each with an associated reference index, ordered by relevance in descending order.

                                            When formulating your response:
                                            1. Include the reference index using square brackets immediately after the relevant information sourced from that reference.
                                            2. Explicitly include line breaks as `\n` within the "Answer" field to preserve the paragraph structure of the original text.

                                            If you cannot find the answer in the context, clearly state that the answer is not available based on the given context.

                                            Your response should be formatted strictly as a JSON object, maintaining the formatting of the original text, including line breaks. The format should be as follows:

                                            {
                                                "Answer": "<your answer with inline citations and explicit line breaks (\n) to preserve formatting>",
                                                "References": <json array of used reference indexes>
                                            }

                                            Ensure the entire output is a valid JSON object, including the 'Answer' and 'References' keys. Do not include any content outside this JSON structure.
                                            """;
    
    private readonly List<ChatMessage> _chatMessages =
    [
        new ChatMessage("Assistant", "Welcome how can I help you?")
    ];

    public IEnumerable<ChatMessage> ChatMessages
        => _chatMessages.Where(m => m.Role != "System")
                        .Where(m => !m.Message.Contains(QuestionContextUseCase.ContextMarker));
    
    private ChatCompletionsOptions ChatCompletionsOptions()
    {
        ChatCompletionsOptions options = new()
                                         {
                                             DeploymentName = "gpt-4-32k",
                                             Temperature = 0,
                                             Messages = {new ChatRequestSystemMessage(_systemMessage.Optimize())}
                                         };

        foreach(var chatRequestMessage in ChatRequestMessages())
        {
            options.Messages.Add(chatRequestMessage);
        }


        return options;
        
        IEnumerable<ChatRequestMessage> ChatRequestMessages()
            => _chatMessages.Select(message =>
                                        message.Role switch
                                        {
                                            "Assistant" => (ChatRequestMessage) new ChatRequestAssistantMessage(message.Message),
                                            "User" => new ChatRequestUserMessage(message.Message),
                                            _ => throw new NotImplementedException()
                                        }
                                   )
                            .ToList();
    }

    public async Task AskQuestion(string question)
    {
        var (prompt, references) = await useCase.Execute(question);

        _chatMessages.Add(new ChatMessage("User", prompt));
        _chatMessages.Add(new ChatMessage("User", question));

        Response<ChatCompletions> response = await openAiClient.GetChatCompletionsAsync(ChatCompletionsOptions());
        var responseMessage = response.Value.Choices[0].Message;

        var answer = JsonSerializer.Deserialize<Response>(responseMessage.Content)!;

        var filteredReferences = answer.References
                                       .Where(index => index < references.Length)
                                       .ToDictionary(index => index, index => references[index]);
        
        _chatMessages.Add(new ChatMessage("Assistant", answer.Answer, filteredReferences));
    }

    private record Response(string Answer, int[] References);
}