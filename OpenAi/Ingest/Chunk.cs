namespace OpenAi.Ingest;

public record Chunk(
    string RawContent,
    int StartPage,
    int EndPage,
    int StartLine,
    int EndLine)
{
    public string EmbeddingContent => RawContent.Replace("\n", string.Empty);

    public static Chunk From(IReadOnlyCollection<(string Content, int PageNumber, int LineNumber)> chunkLines)
        => new(string.Join(Environment.NewLine, chunkLines.Select(l => l.Content)),
               chunkLines.First().PageNumber,
               chunkLines.Last().PageNumber,
               chunkLines.First().LineNumber,
               chunkLines.Last().LineNumber);
}