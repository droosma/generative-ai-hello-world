using Microsoft.AspNetCore.ResponseCompression;

using OpenAi.postgres;
using OpenAi.question;
using OpenAi.Web;
using OpenAi.Web.Components;

var builder = WebApplication.CreateBuilder(args);

builder.Services
       .AddResponseCompression(opts => { opts.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(new[] {"application/octet-stream"}); })
       .AddRazorComponents()
       .AddInteractiveServerComponents();
builder.Services.AddSignalR();

var connectionString = builder.Configuration.GetConnectionString("postgresql") ?? throw new Exception("postgresql not configured");
builder.Services.AddTransient<Database>(_ => new PostgresqlDatabase(connectionString));

builder.Services.AddSingleton(_ => builder.Configuration.GetSection("OpenAISettings").Get<OpenAISettings>()!.Client);
builder.Services.AddTransient<QuestionContextUseCase>();

//builder.Services.AddTransient<IConversation, ConversationWithReferences>();
builder.Services.AddTransient<IConversation, Conversation>();
builder.Services.AddSingleton<Func<IConversation>>(provider => provider.GetRequiredService<IConversation>);

var app = builder.Build();

app.UseResponseCompression()
   .UseExceptionHandler("/Error", createScopeForErrors:true)
   .UseStaticFiles()
   .UseAntiforgery();

app.MapRazorComponents<App>()
   .AddInteractiveServerRenderMode();

app.MapHub<ChatHub>(ChatHub.HubUrl);

app.Run();