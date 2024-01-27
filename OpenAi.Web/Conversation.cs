using Azure;
using Azure.AI.OpenAI;

using OpenAi.question;

namespace OpenAi.Web;

public class Conversation(
    QuestionContextUseCase questionContextUseCase,
    OpenAIClient openAiClient) : IConversation
{
    private const string _systemMessage = $"""
                                            - Role: Helpful Documentation Assistant
                                            - Purpose: Assist consumers with questions specifically about documentation.
                                            - Method: Answer using only the context provided within `{QuestionContextUseCase.ContextMarker}`.
                                            - Context Details:
                                              - Contains relevance-ordered matches.
                                            - Limitations:
                                              - If the answer isn't in the context, clearly state inability to answer.
                                            - Note:
                                              - No need for content warnings in messages; users are pre-informed about reliability.
                                              - Focus solely on answering the question.
                                            """;
    
    #region UI code
    private readonly ChatCompletionsOptions _chatCompletionsOptions = new()
                                                                      {
                                                                          DeploymentName = "gpt-4-32k",
                                                                          Messages =
                                                                          {
                                                                              new ChatRequestSystemMessage(_systemMessage.Optimize()),
                                                                              new ChatRequestAssistantMessage("Welcome how can I help you?")
                                                                          }
                                                                      };

    
    public IEnumerable<ChatMessage> ChatMessages
        => _chatCompletionsOptions.Messages
                                  .Where(m => m is ChatRequestUserMessage or ChatRequestAssistantMessage)
                                  .Select(m => m switch
                                  {
                                      ChatRequestUserMessage userMessage => new ChatMessage("User", userMessage.Content),
                                      ChatRequestAssistantMessage assistantMessage => new ChatMessage("Assistant", assistantMessage.Content)
                                  })
                                  .Where(m => !m.Message.Contains(QuestionContextUseCase.ContextMarker));
    #endregion

    public async Task AskQuestion(string question)
    {
        ////
        // Get context prompt based off of question
        ////
        var contextPrompt = await questionContextUseCase.Execute(question);

        ////
        // Add context prompt and question to chat conversation
        ////
        _chatCompletionsOptions.Messages.Add(new ChatRequestUserMessage(contextPrompt.Item1));
        _chatCompletionsOptions.Messages.Add(new ChatRequestUserMessage(question));

        ////
        // Pass the new Chat history to our LLM
        ////
        Response<ChatCompletions> response = await openAiClient.GetChatCompletionsAsync(_chatCompletionsOptions);
        var responseMessage = response.Value.Choices[0].Message;

        ////
        // Add the response to our chat conversation
        ////
        _chatCompletionsOptions.Messages.Add(new ChatRequestAssistantMessage(responseMessage));
    }
}