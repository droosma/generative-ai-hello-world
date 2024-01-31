using Microsoft.Extensions.Configuration;

using OpenAi.Console;
using OpenAi.Ingest;
using OpenAi.postgres;

const string welcomeText = @"
                 ___  ________   ________  _______   ________  _________  ________  ________     
                |\  \|\   ___  \|\   ____\|\  ___ \ |\   ____\|\___   ___\\   __  \|\   __  \    
                \ \  \ \  \\ \  \ \  \___|\ \   __/|\ \  \___|\|___ \  \_\ \  \|\  \ \  \|\  \   
                 \ \  \ \  \\ \  \ \  \  __\ \  \_|/_\ \_____  \   \ \  \ \ \  \\\  \ \   _  _\  
                  \ \  \ \  \\ \  \ \  \|\  \ \  \_|\ \|____|\  \   \ \  \ \ \  \\\  \ \  \\  \| 
                   \ \__\ \__\\ \__\ \_______\ \_______\____\_\  \   \ \__\ \ \_______\ \__\\ _\ 
                    \|__|\|__| \|__|\|_______|\|_______|\_________\   \|__|  \|_______|\|__|\|__|
                                                       \|_________|                              
                                                                                                 
                                                                                                 
                 _____ ______   ________  ________  ___  ___  ___  ________   _______            
                |\   _ \  _   \|\   __  \|\   ____\|\  \|\  \|\  \|\   ___  \|\  ___ \           
                \ \  \\\__\ \  \ \  \|\  \ \  \___|\ \  \\\  \ \  \ \  \\ \  \ \   __/|          
                 \ \  \\|__| \  \ \   __  \ \  \    \ \   __  \ \  \ \  \\ \  \ \  \_|/__        
                  \ \  \    \ \  \ \  \ \  \ \  \____\ \  \ \  \ \  \ \  \\ \  \ \  \_|\ \       
                   \ \__\    \ \__\ \__\ \__\ \_______\ \__\ \__\ \__\ \__\\ \__\ \_______\      
                    \|__|     \|__|\|__|\|__|\|_______|\|__|\|__|\|__|\|__| \|__|\|_______|      
            ";
Console.WriteLine(welcomeText);

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