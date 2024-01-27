using System.Text.Json;

using Npgsql;
using OpenAi.Ingest;
using OpenAi.question;

using Pgvector;

using Polly;
using Polly.Retry;

using Database=OpenAi.Ingest.Database;

namespace OpenAi.postgres;

public class PostgresqlDatabase : Database, question.Database
{
    private const int _batchSize = 100;
    private readonly Func<ValueTask<NpgsqlConnection>> _connectionFactoryFunc;
    private readonly AsyncRetryPolicy _retryPolicy;

    public PostgresqlDatabase(string connectionString)
    {
        var dataSourceBuilder = new NpgsqlDataSourceBuilder(connectionString);
        dataSourceBuilder.UseVector();
        var dataSource = dataSourceBuilder.Build();

        _retryPolicy = Policy.Handle<PostgresException>(ex => ex.SqlState == "53300")
                             .WaitAndRetryAsync(retryCount: 5,
                                                sleepDurationProvider: attempt => TimeSpan.FromSeconds(Math.Pow(2, attempt)));

        _connectionFactoryFunc = () => dataSource.OpenConnectionAsync();
    }

    public async Task Save(IEnumerable<Embedding> embeddings)
    {
        var embeddingGroups = ChunkEmbeddings(embeddings, _batchSize);
        var tasks = embeddingGroups.Select(SaveChunk);

        await Task.WhenAll(tasks);
    }

    private static IEnumerable<IEnumerable<Embedding>> ChunkEmbeddings(IEnumerable<Embedding> embeddings, int chunkSize)
        => embeddings.Select((e, i) => new { e, i })
                     .GroupBy(x => x.i / chunkSize, x => x.e);

    private async Task SaveChunk(IEnumerable<Embedding> embeddings)
    {
        await _retryPolicy.ExecuteAsync(async () =>
                                        {
                                            await using var connection = await _connectionFactoryFunc();
                                            await using var transaction = await connection.BeginTransactionAsync();
                                            await using var command = new NpgsqlCommand();

                                            command.Connection = connection;
                                            command.Transaction = transaction;

                                            var index = 0;
                                            foreach (var embedding in embeddings)
                                            {
                                                var referenceParam = $"@reference{index}";
                                                var contentParam = $"@content{index}";
                                                var embeddingParam = $"@embedding{index}";

                                                command.CommandText += $"""
                                                                            INSERT INTO embeddings (reference, content, embedding)
                                                                            VALUES ({referenceParam}, {contentParam}, {embeddingParam});
                                                                        """;

                                                command.Parameters.AddWithValue(referenceParam, JsonSerializer.Serialize(embedding.Reference));
                                                command.Parameters.AddWithValue(contentParam, embedding.Content);
                                                command.Parameters.AddWithValue(embeddingParam, new Vector(embedding.Vectors));

                                                index++;
                                            }

                                            await command.ExecuteNonQueryAsync();
                                            await transaction.CommitAsync();
                                        });
    }

    public async Task<IReadOnlyCollection<EmbeddingResult>> Find(ReadOnlyMemory<float> embedding, int numberOfResults = 10)
    {
        await using var connection = await _connectionFactoryFunc();
        await using var command = new NpgsqlCommand("""
                                                        SELECT content, reference
                                                        FROM embeddings
                                                        ORDER BY embedding <-> @embedding
                                                        LIMIT @numberOfResults
                                                    """,
                                                    connection);
        command.Parameters.AddWithValue("numberOfResults", numberOfResults);
        command.Parameters.AddWithValue("embedding", new Vector(embedding));

        await using var reader = await command.ExecuteReaderAsync();

        var records = new List<EmbeddingResult>();

        while (await reader.ReadAsync())
        {
            var content = (string) reader["content"];
            var reference = JsonSerializer.Deserialize<Reference>((string)reader["reference"])!;
            records.Add(new EmbeddingResult(reference, content));
        }

        return records;
    }
}