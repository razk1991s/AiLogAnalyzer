# ⚡ AiLogAnalyzer

> An AI-powered application log analysis tool. Submit raw logs and receive instant structured insights — issue type, severity, explanation, and actionable solutions.

Built to help developers quickly diagnose production issues without manually digging through noisy log output.

---

## 🚀 Tech Stack

| Layer     | Technology                              |
|-----------|-----------------------------------------|
| Frontend  | Angular 17 · TypeScript · SCSS          |
| Backend   | .NET 8 · ASP.NET Core Web API           |
| Database  | MongoDB                                 |
| AI        | OpenAI API (GPT-4o-mini)                |
| Patterns  | DI · Options Pattern · Repository-style |

---

## 📐 Architecture

```
AiLogAnalyzer/
├── frontend/               # Angular 17 SPA
│   └── src/app/
│       ├── components/     # log-analyzer, log-history
│       ├── services/       # LogService (HTTP)
│       ├── models/         # TypeScript interfaces
│       └── pipes/          # severity formatting
│
└── backend/
    └── AiLogAnalyzer.API/
        ├── Controllers/    # LogsController
        ├── Services/       # ILogService, IOpenAiService
        ├── Models/         # LogEntry (MongoDB document)
        ├── DTOs/           # Request/Response records
        └── Configuration/  # MongoDbSettings, OpenAiSettings
```

---

## 🔄 Flow

```
Angular UI
    │
    │  POST /api/logs/analyze  { logText }
    ▼
.NET 8 Web API
    │
    │  Sends log to OpenAI with structured prompt
    ▼
OpenAI GPT-4o-mini
    │
    │  Returns JSON: { issue, severity, explanation, solution }
    ▼
.NET 8 Web API
    │
    │  Saves result to MongoDB
    │  Returns structured response to frontend
    ▼
Angular UI  →  Displays formatted analysis result
```

---

## ⚙️ Getting Started

### Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Node.js 18+](https://nodejs.org/)
- [MongoDB](https://www.mongodb.com/try/download/community) running locally on port `27017`
- OpenAI API key

---

### Backend Setup

```bash
cd backend/AiLogAnalyzer.API
```

Open `appsettings.json` and set your OpenAI API key:

```json
"OpenAiSettings": {
  "ApiKey": "sk-your-api-key-here",
  "Model": "gpt-4o-mini",
  "MaxTokens": 1000
}
```

Run the API:

```bash
dotnet restore
dotnet run
```

The API will be available at `http://localhost:5000`  
Swagger UI: `http://localhost:5000/swagger`

---

### Frontend Setup

```bash
cd frontend
npm install
ng serve
```

The app will be available at `http://localhost:4200`

---

## 📡 API Endpoints

| Method   | Endpoint                    | Description                     |
|----------|-----------------------------|---------------------------------|
| `POST`   | `/api/logs/analyze`         | Analyze a log entry with AI     |
| `GET`    | `/api/logs`                 | Get paginated log history       |
| `GET`    | `/api/logs/{id}`            | Get a single log entry          |
| `DELETE` | `/api/logs/{id}`            | Delete a log entry              |

### Example Request

```http
POST /api/logs/analyze
Content-Type: application/json

{
  "logText": "[ERROR] System.NullReferenceException: Object reference not set to an instance of an object at UserService.GetById(Int32 id)"
}
```

### Example Response

```json
{
  "id": "65a1b2c3d4e5f6a7b8c9d0e1",
  "logText": "[ERROR] System.NullReferenceException...",
  "issue": "Null reference in UserService.GetById",
  "severity": "high",
  "explanation": "A NullReferenceException was thrown when attempting to access a property on an uninitialized object inside UserService. This commonly occurs when a database query returns null and the result is used without a null check.",
  "solution": "• Add null check before accessing the returned object\n• Use the null-conditional operator (?.) for safe access\n• Verify the userId exists before querying\n• Consider returning a 404 NotFound result instead of throwing",
  "createdAt": "2024-01-15T14:23:45Z"
}
```

---

## 🗃️ MongoDB Schema

**Collection:** `logs`

```json
{
  "_id":         "ObjectId",
  "logText":     "string",
  "issue":       "string",
  "severity":    "critical | high | medium | low | info",
  "explanation": "string",
  "solution":    "string",
  "createdAt":   "ISODate"
}
```

---

## 🔮 Future Improvements

- [ ] Authentication (JWT)
- [ ] Log history dashboard with severity charts
- [ ] Bulk log upload (file drag & drop)
- [ ] Export analysis as PDF
- [ ] WebSocket streaming for real-time AI responses
- [ ] Docker Compose setup

---

## 📄 License

MIT
