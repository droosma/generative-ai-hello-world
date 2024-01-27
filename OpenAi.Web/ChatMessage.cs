using Microsoft.AspNetCore.Components;

using OpenAi.Ingest;

namespace OpenAi.Web;

public record ChatMessage(
    string Role,
    string Message,
    IDictionary<int, Reference>? References = null)
{
    public MarkupString MessageMarkup => new(Message.Replace("\n", "<br>"));
    public IDictionary<int, Reference>? References { get; set; } = References;
}