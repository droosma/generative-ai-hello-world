using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using OpenAi.Console;

using var host = Host.CreateApplicationBuilder(args).Build();
var config = host.Services.GetRequiredService<IConfiguration>();

var action = string.Empty;
do
{
    Console.WriteLine("What do you want to do? (ingest, createindex, search, exit)");
    action = Console.ReadLine();

    switch(action)
    {
        case "ingest":
            await Ingest.Execute(config);
            break;
        case "createindex":
            CreateIndex.Execute(config);
            break;
        case "search":
            await Search.Execute(config);
            break;
        case "exit":
            return;
        default:
            Console.WriteLine("Unknown command");
            break;
    }
}
while(action != "exit");

await host.RunAsync();