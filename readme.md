# AnalyticsPipeline - .NET Core Backend

A small .NET Core backend system that reads mocked Google Analytics (GA) and PageSpeed Insights (PSI) JSON files, publishes data to RabbitMQ, aggregates statistics in SQL Server, and exposes JWT-protected reporting APIs.

---

## **Table of Contents**

- [Features](#features)  
- [Requirements](#requirements)  
- [Project Structure](#project-structure)  
- [Setup](#setup)  
- [Running the Project](#running-the-project)  
- [Database](#database)  
- [RabbitMQ](#rabbitmq)  
- [API Endpoints](#api-endpoints)  
- [Authentication](#authentication)  

---

## **Features**

- Read GA & PSI JSON files and combine into unified records  
- Publish records to RabbitMQ (real message broker)  
- Background worker consumes messages and aggregates daily stats  
- Persist raw and aggregated data to SQL Server  
- JWT-protected reporting APIs:
  - `/api/reports/overview` → totals across all pages & dates  
  - `/api/reports/pages` → totals/averages grouped by page  
- User registration & login with email/password and JWT  

---

## **Requirements**

- [.NET 8 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/8.0)  
- [SQL Server](https://www.microsoft.com/en-us/sql-server) (Express or full)  
- [RabbitMQ](https://www.rabbitmq.com/download.html)  
- [Postman](https://www.postman.com/) or Swagger UI for API testing  

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
│   └─ AuthService.cs
│
├─ appsettings.json
├─ Program.cs
└─ README.md
```

---

## **Setup**

1. Clone the repository:

```bash
git clone https://github.com/RashaElnaggar/analytics-pipeline-dotnet.git
cd analytics-pipeline-dotnet
```

2. Install dependencies:

```bash
dotnet restore
```

3. Update `appsettings.json` with your **SQL Server connection** and **JWT settings**:

```json
"ConnectionStrings": {
  "DefaultConnection": "Server=YOUR_SERVER;Database=AnalyticsDb;User Id=YOUR_USERNAME;Password=YOUR_PASSWORD;"
},
"Jwt": {
  "Key": "this_is_a_super_secret_key_32_chars!",
  "Issuer": "https://localhost:7075",
  "Audience": "https://localhost:7075"
}
```

4. Ensure RabbitMQ is installed and running on your machine.  

---

## **Running the Project**

```bash
dotnet run
```

- The app will run at:  
```
http://localhost:5144
```

- Swagger UI (API documentation & testing) is available at:  
```
http://localhost:5144/swagger/index.html
```

---

## **Database**

- The project uses **EF Core** with SQL Server  
- Automatically runs migrations on startup (dev environment)  
- Tables created:
  - `Users` → stores registered users  
  - `RawData` → stores ingested GA & PSI data  
  
  - `DailyStats` → stores aggregated daily stats  

---

## **RabbitMQ**

- Exchanges & queues:
  - `analytics.raw` → main exchange for raw records  
  - `analytics.raw.q` → queue bound to the exchange  
- Background worker consumes messages from the queue and aggregates data  

---

## **API Endpoints**

### **Auth (JWT)**

| Method | Endpoint             | Description                       |
|--------|--------------------|-----------------------------------|
| POST   | /api/auth/register  | Register a new user               |
| POST   | /api/auth/login     | Login and receive JWT token       |

**Example Login Request Body:**

```json
{
  "email": "test@example.com",
  "passwordHash": "123456"
}
```

**Example Response:**

```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..."
}
```

---

### **Reports (JWT Protected)**

> Add `Authorization: Bearer <your-token>` in headers  

| Method | Endpoint                     | Description                               |
|--------|------------------------------|-------------------------------------------|
| GET    | /api/reports/overview        | Total users, sessions, views, avg performance |
| GET    | /api/reports/pages           | Totals/averages grouped by page           |

---

## **Authentication**

- Use JWT Bearer tokens to access reports  
- Steps:
  1. Register a user via `/api/auth/register`  
  2. Login via `/api/auth/login` to get JWT token  
  3. Use the token in the **Authorization header** for report endpoints  

**Header Example:**

```
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

---

## **Testing**

- Use **Swagger UI**: `http://localhost:5144/swagger/index.html`  
- Use **Postman** to call endpoints with JWT  
- Ensure RabbitMQ and SQL Server are running before testing  

---

## **License**

MIT License – feel free to modify and use.

