# ProcessMonitor.API

Process Monitor API evaluates whether actions taken during a process comply with established guidelines using a simple AI model integration.

⚡ **You can run this API either via Docker or by building and running it traditionally with .NET 10 SDK.**



## Running the API

---

## Prerequisites

Before running the API, ensure you have the following:

* Optional: [Postman](https://www.postman.com/) for testing API endpoints
* Optional: Companion console app for testing: [ProcessMonitor.App](https://github.com/ivanadal/ProcessMonitor.App)
* Environment Variables:

  * `HuggingFaceApiKey` – token from [huggingface.co](https://huggingface.co)
  * `ApiKey` – key for ProcessMonitorAPI authorization (used for mocking authorization) 
  - Note: This is a small project and the API key is only for demonstration and local testing purposes. In production systems, you would normally use OAuth 2.0, JWT tokens, or another robust authentication/authorization mechanism instead of a static API key.

---

### **Option 1: Using Docker**

```bash
# Build the Docker image
DOCKER:
docker build -t processmonitorapi .
PODMAN:
podman build -t processmonitorapi:test . 

# Run the container
DOCKER:
docker run --rm -it -p 8080:80 `
    -e ApiKey="testsecret" `
    -e HuggingFaceApiKey="test" `
    processmonitorapi:test
PODMAN:
podman run --rm -it -p 8080:80 `
    -e ApiKey="testsecret" `
    -e HuggingFaceApiKey="test" `
    processmonitorapi:test
```
*NOTE: I have an old machine, so I couldn't run docker. Instead I have using podman. Therefore this is tested with podman, but it should be working with Docker as well.

The API will be exposed on `http://localhost:5000` inside the container.

---

### **Option 2: Traditional .NET Build and Run**

---

## Prerequisites

Before running the API, ensure you have the following:

* [.NET SDK 10.x](https://dotnet.microsoft.com/en-us/download/dotnet)
* [Visual Studio 2022+](https://visualstudio.microsoft.com/) or [VS Code](https://code.visualstudio.com/)
* [SQLite](https://www.sqlite.org/download.html)
* Optional: [Postman](https://www.postman.com/) for testing API endpoints
* Optional: Companion console app for testing: [ProcessMonitor.App](https://github.com/ivanadal/ProcessMonitor.App)
* Environment Variables:

  * `HuggingFaceApiKey` – token from [huggingface.co](https://huggingface.co)
  * `ApiKey` – key for ProcessMonitorAPI authorization (used for mocking authorization) 
  - Note: This is a small project and the API key is only for demonstration and local testing purposes. In production systems, you would normally use OAuth 2.0, JWT tokens, or another robust authentication/authorization mechanism instead of a static API key.

---

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

4. **Configure environment and appsettings**

* The project includes DEV, Staging, and Production appsettings (currently treated the same).
* Update `LogFilePath` and `HuggingFace` model if needed:

```json
"HuggingFace": {
  "ModelId": "facebook/bart-large-mnli",
  "Endpoint": "https://router.huggingface.co/hf-inference/models",
  "CandidateLabels": ["complies", "deviates", "unclear"]
},

"LogFilePath": "C:/Logs/app.log"
```

5. **Apply database migrations**

```bash
dotnet ef database update
```

6. **Run the API**

```bash
dotnet run
```

By default, the API will run on:

* `https://localhost:7023`
* `http://localhost:5215`

You can adjust ports in `Properties/launchSettings.json`.

---

## API Endpoints

| Method | Endpoint | Description                        |
| ------ | -------- | ---------------------------------- |
| POST   | /analyze | Submit analysis request            |
| GET    | /summary | Get aggregated analysis statistics |
| GET    | /history | Get analysis history               |

---

## Testing

Run unit tests:

```bash
dotnet test
```

---

## Logging

Logs are output to the console by default. You can configure logging in `appsettings.json`.

---

### Debugging Tip
When debugging in Visual Studio, the debugger may pause on thrown exceptions (first-chance exceptions) even though they are properly caught by the global exception middleware.

This is expected behavior and does not indicate a crash.

To disable pausing:
- Go to **Debug > Windows > Exception Settings**
- Uncheck **Common Language Runtime Exceptions** (or uncheck "Thrown" for CLR exceptions)

When running with `dotnet run` or in production, exceptions are handled normally with custom JSON responses.
