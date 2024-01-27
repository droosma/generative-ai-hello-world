namespace OpenAi.Web;

public interface IConversation
{
    IEnumerable<ChatMessage> ChatMessages { get; }
    Task AskQuestion(string question);
}