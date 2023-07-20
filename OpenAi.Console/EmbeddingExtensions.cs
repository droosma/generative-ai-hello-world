namespace OpenAi.Console;

public static class EmbeddingExtensions
{
    public static byte[] AsBytes(this IReadOnlyCollection<float> vectors)
    {
        var vectorBytes = new byte[vectors.Count * sizeof(float)];
        Buffer.BlockCopy(vectors.ToArray(), 0, vectorBytes, 0, vectorBytes.Length);
        return vectorBytes;
    }
}