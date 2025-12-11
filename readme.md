# ProcessMonitor.API

Process Monitor API that evaluates whether actions taken during a process comply with established guidelines using a simple AI model integration.

## Prerequisites

Before running the API, ensure you have the following installed:

* [.NET SDK 10.x](https://dotnet.microsoft.com/en-us/download/dotnet)
* [Visual Studio 2022+](https://visualstudio.microsoft.com/) or [VS Code](https://code.visualstudio.com/)
* [SQLite](https://www.sqlite.org/download.html)
* Optional: [Postman](https://www.postman.com/) for testing API endpoints
* You can also test the API using the companion console application: [ProcessMonitor.App](https://github.com/ivanadal/ProcessMonitor.App)
* Add ENVIRONMENT VARIABLES:
    * **HuggingFaceApiKey** - token from  huggingface.co.
    * **ApiKey** - something that will be key for current API, to mock Autorization
  
## Getting Started

1. **Clone the repository**

```bash
git clone https://github.com/ivanadal/ProcessMonitor.API
cd ProcessMonitor.API
```

2. **Restore dependencies**

```bash
dotnet restore
```

3. **Build the project**

```bash
dotnet build
```

4. **Configure the environment**

* There are DEV, Staging and production appsettings, but currently we are treating them same
* Update LogFilePath, and ModelId if you plan to use some other model

```json
  "HuggingFace": {
    "ModelId": "facebook/bart-large-mnli",
    "Endpoint": "https://router.huggingface.co/hf-inference/models",
    "CandidateLabels": [ "complies", "deviates", "unclear" ]
  },

  "LogFilePath": "C:/Logs/app.log"
```

5. **Run database migrations** 

```bash
dotnet ef database update
```

6. **Run the API**

```bash
dotnet run
```

By default, the API will run on `https://localhost:7023` and `http://localhost:5215`. (If needed you can change it within launchSettings.json)

## API Endpoints

| Method | Endpoint | Description                        |
| ------ | -------- | ---------------------------------- |                
| POST   | /analyze | Submit analysis request            |
| GET    | /summary | Get aggregated analysis statistics |
| GET    | /history | Get analysis history               |

## Testing

To run unit tests:

```bash
dotnet test
```

## Logging

Logs are output to the console by default. You can configure logging in `appsettings.json`.

## Docker (Optional)

To build and run with Docker:

```bash
docker build -t processmonitorapi .
docker run -p 5000:80 project-name
```

