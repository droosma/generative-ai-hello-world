namespace OpenAi.Ingest;

public record Reference(
    string FileName,
    string Version,
    int StartPage,
    int EndPage,
    int StartLine,
    int EndLine)
{
    public static Reference From(Partition partition)
        => new("enbrel-epar-product-information_en.pdf",
               "v1",
               partition.StartPage,
               partition.EndPage,
               partition.StartLine,
               partition.EndLine);
}