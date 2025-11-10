# AnalyticsPipeline

A small backend system that reads mock JSON data (Google Analytics & PageSpeed Insights), publishes it via RabbitMQ, aggregates it in SQL Server, and exposes reporting APIs secured with JWT.

---

## **Project Structure**

```
AnalyticsPipeline/
│
├─ Controllers/
│   ├─ AuthController.cs
│   └─ ReportsController.cs
│
├─ Data/
│   └─ AnalyticsDbContext.cs
│
├─ Models/
│   ├─ User.cs
│   ├─ RawData.cs
│   └─ DailyStats.cs
│
├─ Services/
│   ├─ AuthService.cs
│   └─ DataLoader.cs
│
├─ MockData/
│   ├─ ga.json
│   └─ psi.json
│
├─ appsettings.json
├─ Program.cs
├─ Dockerfile
├─ docker-compose.yml
└─ README.md
```

---

## **1️⃣ Docker Setup**

Ensure Docker Desktop is installed and running.

### Build and run all services:
```bash
docker-compose up --build -d
```

This will start:

- **API** → `http://localhost:7075/swagger/index.html`  
- **SQL Server** → `localhost:1433` (user: `sa`, password: `YourStrong!Passw0rd`)  
- **RabbitMQ** → `http://localhost:15672` (user: `guest`, password: `guest`)  

### Stop services:
```bash
docker-compose down
```

---

```json
"ConnectionStrings": {
  "DefaultConnection": "Server=YOUR_SERVER;Database=AnalyticsDb;User Id=YOUR_USERNAME;Password=YOUR_PASSWORD;"
},
"Jwt": {
  "Key": "this_is_a_super_secret_key_32_chars!",
  "Issuer": "https://localhost:5144",
  "Audience": "https://localhost:5144"
}
```

Mock JSON files are in `MockData/`:

- **Google Analytics (ga.json)**

```json
[
  { "date": "2025-10-20", "page": "/home", "users": 120, "sessions": 150, "views": 310 }
]
```

- **PageSpeed Insights (psi.json)**

```json
[
  { "date": "2025-10-20", "page": "/home", "performanceScore": 0.9, "LCP_ms": 2100 }
]
```

To seed data into the database:

```http
POST http://localhost:7075/api/Ingestion/publish
```

- This reads JSON → publishes to RabbitMQ → background consumer → inserts into SQL Server → updates aggregated `DailyStats`.

---

## **3️⃣ Swagger & JWT Testing**

1. Open Swagger UI:

```
http://localhost:7075/swagger/index.html
```

2. **Signup/Login** via `/api/Auth/signup` and `/api/Auth/login`:

- `POST /api/Auth/signup` with JSON body:

```json
{
  "name": "Rasha",
  "email": "rasha@example.com",
  "password": "Test123!"
}
```

- `POST /api/Auth/login` with JSON body:

```json
{
  "email": "rasha@example.com",
  "password": "Test123!"
}
```

- Copy the returned **JWT token**.

3. **Authorize requests**:

- In Swagger, click **Authorize** → paste `Bearer <your-token>` → execute `/reports/overview` or `/reports/pages`.

---

## **4️⃣ Reports API**

- **GET /api/Reports/overview** → aggregated totals across all pages & dates.
- **GET /api/Reports/pages** → aggregated totals per page.

Both endpoints require **JWT Bearer token**.

---

## **5️⃣ Database**

- **Users**: Id, Name, Email, PasswordHash, CreatedAt  
- **RawData**: Id, Page, Date, Users, Sessions, Views, PerformanceScore, LCPms, ReceivedAt  
- **DailyStats**: Id, Date, TotalUsers, TotalSessions, TotalViews, AvgPerformance, LastUpdatedAt

---

## **6️⃣ Notes**

- Docker maps host port 7075 → container port 80. Endpoints remain unchanged.  
- RabbitMQ UI: `http://localhost:15672` (guest/guest)  
- SQL Server can be accessed with your favorite client (SSMS, Azure Data Studio) using `localhost,1433`.

---

## **7️⃣ Troubleshooting**

- If API returns **404 on reports**, make sure you’ve ingested JSON data first (`/api/Ingestion/publish`).  
- If 401 Unauthorized → ensure you are passing **JWT Bearer token** in Authorization header.  
- Logs can be checked in Docker Desktop or via:

```bash
docker-compose logs -f
```
