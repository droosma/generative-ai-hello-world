namespace OpenAi.Ingest;

public interface FileSystem
{
    public Task<Stream> Load(string path);
}

public class LocalFileSystem : FileSystem
{
    public async Task<Stream> Load(string path)
    {
        await using var fileStream = File.OpenRead(path);
        var memStream = new MemoryStream();
        await fileStream.CopyToAsync(memStream);
        memStream.Position = 0;

        return memStream;
    }
}