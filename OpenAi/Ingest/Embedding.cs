namespace OpenAi.Ingest;

public record Embedding(
    Reference Reference,
    string Content,
    ReadOnlyMemory<float> Vectors)
{
    public static Embedding From(Partition partition, ReadOnlyMemory<float> embedding)
        => new(Reference.From(partition), partition.RawContent, embedding);
}