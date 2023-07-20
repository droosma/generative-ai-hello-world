using Microsoft.Extensions.Configuration;

using NRedisStack;
using NRedisStack.RedisStackCommands;
using NRedisStack.Search;
using NRedisStack.Search.Literals.Enums;

namespace OpenAi.Console;

public static class CreateIndex
{
    public static void Execute(IConfiguration config)
    {
        var redisSettings = config.GetSection("RedisPersistence")
                                  .Get<RedisSettings>() ?? throw new ArgumentException("RedisPersistence configuration failed");
        var databaseClient = redisSettings.Database;

        ISearchCommands ft = databaseClient.FT();
        ft.Create(redisSettings.Index,
                  new FTCreateParams().On(IndexDataType.HASH)
                                      .Prefix($"{redisSettings.Index}:"),
                  new Schema().AddVectorField("embedding",
                                              Schema.VectorField.VectorAlgo.HNSW,
                                              new Dictionary<string, object>
                                              {
                                                  {"TYPE", "FLOAT32"},
                                                  {"DIM", 1536},
                                                  {"DISTANCE_METRIC", "COSINE"}
                                              })
                              .AddTextField("content"));

        System.Console.WriteLine("Index created");
    }
}