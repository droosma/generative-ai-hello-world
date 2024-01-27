using Microsoft.Extensions.Configuration;

using OpenAi.Console;
using OpenAi.Ingest;
using OpenAi.postgres;

var configuration = new ConfigurationBuilder()
                    .AddJsonFile("appsettings.json")
                    .AddUserSecrets<Program>()
                    .Build();

var openAiSettings = OpenAISettings.From(configuration.GetSection(nameof(OpenAISettings)));
var documentAnalysisSettings = DocumentAnalysisSettings.From(configuration.GetSection(nameof(DocumentAnalysisSettings)));

var ingestionUseCase = new IngestionUseCase(openAiSettings.Client,
                                            documentAnalysisSettings.DocumentAnalysisClient,
                                            new LocalFileSystem(),
                                            new PostgresqlDatabase(configuration.GetConnectionString("postgresql")!));

await ingestionUseCase.Execute();

partial class Program
{
}