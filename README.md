# Document Q&A with Azure OpenAI

This is a simple example of how to use Azure OpenAI to ask questions about a document that GPT is not aware of. I have made this as straightforward as possible to facilitate understanding of the code and its integration into your projects. However, this simplicity means the example does not adhere to my typical standards for production code.

## Requirements

At the time of writing, Azure OpenAI is in private preview. You can request access [here](https://customervoice.microsoft.com/Pages/ResponsePage.aspx?id=v4j5cvGGr0GRqy180BHbR7en2Ais5pxKtso_Pz4b1_xUOFA5Qk1UWDRBMjg0WFhPMkIzTzhKQ1dWNyQlQCN0PWcu).
This code uses a PDF as its source document and requires extraction of raw text. For this, I use the Form Recognizer resource in Azure. You can create one [here](https://portal.azure.com/#create/Microsoft.CognitiveServicesFormRecognizer).
If you already have the raw text, you can bypass this requirement.

## How to run

This project uses Postgresql as its vector database, so you need a running Postgresql instance. A simple way to set this up is by running the following command in a terminal:

```powershell
docker-compose up -d
```

## OpenAi.Console

To execute the workflow successfully, configure the correct `appsettings.json`

```JSON
{
    "OpenAISettings": {
        "EndPoint": "https://[The-Name-You-Gave-Your-Azure-OpenAI-Resource].openai.azure.com/"
    },
    "DocumentAnalysisSettings": {
        "EndPoint": "https://[ths-cognative-searvices-name].cognitiveservices.azure.com/"
    }
}
```

And add the following to your `secrets.json` (more info here: [app-secrets](https://learn.microsoft.com/en-us/aspnet/core/security/app-secrets?view=aspnetcore-8.0))

```JSON
    "OpenAISettings": {
        "Key": "<YOUR KEY>"
    },
    "DocumentAnalysisSettings": {
        "Key": "<YOUR KEY>"
    },
    "ConnectionStrings": {
        "Postgresql": "Host=localhost;Database=postgres;Username=user;Password=password;Pooling=true;MinPoolSize=1;MaxPoolSize=100;"
    }
```

## OpenAi.Web

```JSON
{
    "OpenAISettings": {
        "EndPoint": "https://[The-Name-You-Gave-Your-Azure-OpenAI-Resource].openai.azure.com/"
    }
}
```

And add the following to your `secrets.json` (more info here: [app-secrets](https://learn.microsoft.com/en-us/aspnet/core/security/app-secrets?view=aspnetcore-8.0))

```JSON
    "OpenAISettings": {
        "Key": "<YOUR KEY>"
    },
    "ConnectionStrings": {
        "Postgresql": "Host=localhost;Database=postgres;Username=user;Password=password;Pooling=true;MinPoolSize=1;MaxPoolSize=100;"
    }
```