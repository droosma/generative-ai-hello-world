namespace OpenAi.Ingest;

public interface Database
{
    public Task Save(IEnumerable<Embedding> embeddings);
}