# Document Q&A with Azure OpenAI

This is a simple example of how to use Azure OpenAI to ask questions about a document that GPT is not aware of. I have made this as straightforward as possible to facilitate understanding of the code and its integration into your projects. However, this simplicity means the example does not adhere to my typical standards for production code.

## Requirements

At the time of writing, Azure OpenAI is in private preview. You can request access [here](https://customervoice.microsoft.com/Pages/ResponsePage.aspx?id=v4j5cvGGr0GRqy180BHbR7en2Ais5pxKtso_Pz4b1_xUOFA5Qk1UWDRBMjg0WFhPMkIzTzhKQ1dWNyQlQCN0PWcu).
This code uses a PDF as its source document and requires extraction of raw text. For this, I use the Form Recognizer resource in Azure. You can create one [here](https://portal.azure.com/#create/Microsoft.CognitiveServicesFormRecognizer).
If you already have the raw text, you can bypass this requirement.

## How to run

This project uses Redis as its vector database, so you need a running Redis instance. A simple way to set this up is by running the following command in a terminal:

```powershell
docker run --rm -it -p 6379:6379 -p 8001:8001 redis/redis-stack:latest
```

To execute the workflow successfully, configure the correct appsettings.json.

```JSON
{
    "OpenAI": {
        "key": "KEY 1 or KEY 2 found Resource Management > Keys and Endpoint",
        /// To use OpenAI set Endpoint to an empty string
        "endpoint": "https://[The-Name-You-Gave-Your-Azure-OpenAI-Resource].openai.azure.com/"
    },
    "FormRecognizer": {
        "key": "KEY 1 or KEY 2 found Resource Management > Keys and Endpoint",
        "endpoint": "https://[ths-cognative-searvices-name].cognitiveservices.azure.com/"
    },
    "RedisPersistence": {
        "ConnectionString": "localhost:6379",
        "Index": "documents"
    }
}
```

## Example run flow

0. Configure `appsettings.json` & Start Redis
1. Navigate to `.\OpenAi.Console`
2. Run `dotnet run`
3. Wait for the application to start
4. enter: `createindex` to create the vector index on Redis
5. enter: `ingest` this will ingest the PDF and store the vector in Redis
6. enter: `search` and enter: `What are the side effects of etanercept`