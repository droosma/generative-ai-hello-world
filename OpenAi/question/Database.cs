using OpenAi.Ingest;

namespace OpenAi.question;

public interface Database
{
    public Task<IReadOnlyCollection<EmbeddingResult>> Find(ReadOnlyMemory<float> embedding, int numberOfMatches);
}

public record EmbeddingResult(Reference Reference, string Content);