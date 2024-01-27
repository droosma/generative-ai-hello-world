using System.Collections.Concurrent;

using Azure;
using Azure.AI.OpenAI;

using Microsoft.AspNetCore.SignalR;

namespace OpenAi.Web;

public record OpenAISettings(string Key, Uri EndPoint)
{
    public OpenAIClient Client => new(EndPoint, new AzureKeyCredential(Key));
}

public class ChatHub(Func<IConversation> conversationFactory) : Hub
{
    public const string HubUrl = "/chat";

    public const string ClientReceiveChatMessages = "ReceiveChatMessages";
    public const string SendNewMessageName = "ReceiveNewMessage";

    private static readonly ConcurrentDictionary<string, IConversation> _chatConversations = new();

    public override async Task OnConnectedAsync()
    {
        _chatConversations.TryAdd(Context.ConnectionId, conversationFactory.Invoke());

        await Clients.Caller.SendAsync(ClientReceiveChatMessages, _chatConversations[Context.ConnectionId].ChatMessages);

        await base.OnConnectedAsync();
    }

    public Task ResetHistory()
    {
        _chatConversations[Context.ConnectionId] = conversationFactory.Invoke();

        return SendMessages();
    }

    private Task SendMessages() => Clients.Caller.SendAsync(ClientReceiveChatMessages, _chatConversations[Context.ConnectionId].ChatMessages);

    public override Task OnDisconnectedAsync(Exception? exception)
    {
        _chatConversations.TryRemove(Context.ConnectionId, out _);
        return base.OnDisconnectedAsync(exception);
    }

    public async Task ReceiveNewMessage(string message)
    {
        await _chatConversations[Context.ConnectionId].AskQuestion(message);
        await SendMessages();
    }
}