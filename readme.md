# ProcessMonitor.API

Process Monitor API evaluates whether actions taken during a process comply with established guidelines using a simple AI model integration.

⚡ **You can run this API either via Docker or by building and running it traditionally with .NET 10 SDK.**

---

## Prerequisites

Before running the API, ensure you have the following installed:

* [.NET SDK 10.x](https://dotnet.microsoft.com/en-us/download/dotnet)
* [Visual Studio 2022+](https://visualstudio.microsoft.com/) or [VS Code](https://code.visualstudio.com/)
* [SQLite](https://www.sqlite.org/download.html)
* Optional: [Postman](https://www.postman.com/) for testing API endpoints
* Optional: Companion console app for testing: [ProcessMonitor.App](https://github.com/ivanadal/ProcessMonitor.App)
* Environment Variables:

  * `HuggingFaceApiKey` – token from [huggingface.co](https://huggingface.co)
  * `ApiKey` – key for API authorization (used for mocking authorization)

---

## Running the API

### **Option 1: Using Docker**

```bash
# Build the Docker image
docker build -t processmonitorapi .

# Run the container
docker run --rm -it -p 8080:80 `
    -e ApiKey="testsecret" `
    -e HuggingFaceApiKey="test" `
    processmonitorapi:test
```
*NOTE: I have an old machine, so I couldn't run docker. Instead I have using podman. Therefore this is tested with podman, but it should be working with Docker as well.

The API will be exposed on `http://localhost:5000` inside the container.

> ⚠️ On Windows, make sure to add a `.dockerignore` file to exclude `.vs`, `bin`, `obj`, and other temporary files to avoid permission issues.

---

### **Option 2: Traditional .NET Build and Run**

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

## Notes

* The API uses SQLite by default.
* Use environment variables to avoid storing sensitive data in `appsettings.json`.
* The companion console app can be used to send requests and test endpoints without Postman.
