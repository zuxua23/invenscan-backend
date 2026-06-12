<p align="center">
  <img src="https://img.shields.io/badge/ASP.NET_Core-8.0-512BD4?style=for-the-badge&logo=dotnet" />
  <img src="https://img.shields.io/badge/SQL_Server-CC2927?style=for-the-badge&logo=microsoftsqlserver&logoColor=white" />
  <img src="https://img.shields.io/badge/Entity_Framework_Core-8.0-512BD4?style=for-the-badge" />
  <img src="https://img.shields.io/badge/JWT-Auth-000000?style=for-the-badge&logo=jsonwebtokens" />
</p>

<h1 align="center">InvenScan Backend</h1>
<p align="center">Enterprise inventory management backend — REST API + Web Dashboard</p>
<p align="center">by <a href="https://github.com/zuxua23">Zuxlabs</a></p>

---

## Overview

InvenScan Backend is the server-side component of the InvenScan inventory management system. It provides a REST API for the Android HT app and a web-based admin dashboard for managing inventory operations.

### Key Features

- **Stock In** — receive items into warehouse locations
- **Stock Out** — record items leaving via Android HT or fixed gate reader
- **Stock Taking** — physical inventory count sessions
- **Stock Preparation** — picking list management for outbound orders
- **Item Search** — real-time item lookup by barcode or RFID EPC
- **Gate Reader** — abstract endpoint for any RFID gate reader brand
- **Transaction History** — full audit trail with Excel/PDF/CSV export
- **Activity Log** — complete audit log for all web and Android actions
- **Dark/Light Theme** — per-user theme preference
- **Multi-language** — full English UI

---

## Requirements

| Requirement | Version |
|-------------|---------|
| .NET SDK | 8.0+ |
| SQL Server | 2019+ (or SQL Server Express) |
| Visual Studio | 2022+ (or VS Code with C# extension) |

---

## Getting Started

### 1. Clone the repository

```bash
git clone https://github.com/zuxua23/invenscan-backend.git
cd invenscan-backend
```

### 2. Configure the database

Open `appsettings.json` and update the connection string:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=YOUR_SERVER;Database=InvenScanDB;User Id=YOUR_USER;Password=YOUR_PASSWORD;TrustServerCertificate=True"
  },
  "JwtSettings": {
    "SecretKey": "YOUR_SECRET_KEY_MIN_32_CHARACTERS",
    "Issuer": "InvenScan",
    "Audience": "InvenScanApp",
    "ExpiryMinutes": 1440
  }
}
```

> **Tip:** You can also configure the database from the login page by clicking the **DB Settings** button.

### 3. Run database migration

```bash
dotnet ef database update
```

This creates all tables and seeds default data:
- Default admin user: `admin` / `admin123`
- Sample items and locations

### 4. Run the application

```bash
dotnet run
```

The application will be available at:
- Web Dashboard: `https://localhost:5001`
- API Base URL: `https://localhost:5001/api`

---

## Default Credentials

| Role | Username | Password |
|------|----------|----------|
| Admin | `admin` | `admin123` |
| Operator | `operator` | `operator123` |

> **Important:** Change default passwords immediately after first login.

---

## Project Structure

```
InvenScan/
├── Controllers/
│   ├── Api/                    # REST API endpoints (JWT auth)
│   │   ├── AuthController.cs
│   │   ├── ItemApiController.cs
│   │   ├── LocationApiController.cs
│   │   ├── TagApiController.cs
│   │   ├── StockInController.cs
│   │   ├── StockOutController.cs
│   │   ├── StockTakingController.cs
│   │   ├── StockPrepController.cs
│   │   ├── SearchItemController.cs
│   │   ├── GateController.cs   # Gate reader endpoint (API Key auth)
│   │   ├── ConfigApiController.cs
│   │   └── PingController.cs
│   └── Web/                    # Web dashboard controllers
│       ├── AuthWebController.cs
│       ├── HomeWebController.cs
│       ├── ItemWebController.cs
│       ├── LocationWebController.cs
│       ├── StockTakingWebController.cs
│       ├── StockOutWebController.cs
│       ├── StockPrepWebController.cs
│       ├── TransactionHistoryWebController.cs
│       ├── ActivityLogWebController.cs
│       ├── GateWebController.cs
│       └── UserWebController.cs
├── Entity/                     # EF Core entities
├── Service/
│   ├── Interfaces/             # Service interfaces
│   └── Implementations/        # Service implementations
├── DTO/
│   ├── Request/                # Request DTOs
│   └── Response/               # Response DTOs
├── Database/                   # AppDbContext
├── Utility/                    # Helpers, constants, JWT
├── Migrations/                 # EF Core migrations
└── wwwroot/                    # Static assets
```

---

## API Reference

### Authentication

All API endpoints require JWT Bearer token except `/api/auth/login` and `/api/ping`.

```http
Authorization: Bearer {token}
```

#### Login
```http
POST /api/auth/login
Content-Type: application/json

{
  "username": "admin",
  "password": "admin123"
}
```

Response:
```json
{
  "success": true,
  "token": "eyJhbGci...",
  "userId": "1",
  "username": "admin",
  "role": "ADMIN"
}
```

---

### Items

```http
GET /api/item                    # List all items
GET /api/item/{id}               # Get item by ID
```

---

### Locations

```http
GET /api/location                # List all locations
```

---

### Tags

```http
GET  /api/tag/{id}               # Get tag by EPC or TagId
POST /api/tag/register           # Register new tags
```

Register request:
```json
{
  "tags": [
    { "epcTag": "E2003412B12", "itemId": 1, "locationId": 1 }
  ]
}
```

---

### Stock In

```http
GET  /api/stockin?code={code}&scannerType={RFID|BARCODE}   # Lookup item
POST /api/stockin                                           # Submit stock in
POST /api/stockin/bulk-info                                 # Bulk lookup
```

Submit request:
```json
{
  "locationId": 1,
  "notes": "Morning receiving",
  "items": [
    { "tagId": "E2003412B12", "itemId": 1, "scanType": "RFID" }
  ]
}
```

---

### Stock Out

```http
GET  /api/stockout?code={code}&scannerType={RFID|BARCODE}  # Lookup item
POST /api/stockout                                          # Submit stock out
POST /api/stockout/bulk-info                                # Bulk lookup
```

---

### Stock Taking

```http
POST /api/stock-taking                          # Create session
GET  /api/stock-taking                          # List all sessions
GET  /api/stock-taking/active                   # Get active session
GET  /api/stock-taking/tags/{sttId}             # Get session items
GET  /api/stock-taking/available-tags/{sttId}   # Available tags
POST /api/stock-taking/operator-submit          # Submit scan results
```

---

### Stock Preparation

```http
GET  /api/stockprep              # List picking lists
GET  /api/stockprep/{id}         # Get picking list detail
POST /api/stockprep/bulk         # Submit picked items
```

---

### Search Item

```http
GET /api/search-item             # List all items (for cache)
GET /api/search-item/{code}      # Search by barcode or EPC
```

---

### Gate Reader (Stock Out via Fixed Reader)

```http
POST /api/gate/stockout
X-Gate-Api-Key: {your-gate-api-key}
Content-Type: application/json

{body depends on your reader's output format}
```

Response:
```json
{
  "processed": 5,
  "unknown": 1,
  "gateCode": "GATE-01"
}
```

> The gate endpoint accepts any JSON format. Configure the field mapping in the web dashboard under **Gate Monitor**.

---

### Ping

```http
GET /api/ping    # Health check — no auth required
```

---

## Gate Reader Setup

1. Go to **Gate Monitor** in the web dashboard
2. Click **Add Gate**
3. Fill in gate name and location
4. Click **Generate API Key** — save this key
5. Configure **Field Mapping** to match your reader's output format
6. Point your reader to `POST https://your-server/api/gate/stockout`
7. Add header `X-Gate-Api-Key: {your-key}`

Supported reader brands (via field mapping): Impinj, Zebra FX/FR series, Alien Technology, ThingMagic, and any reader that can send HTTP POST requests.

---

## Activity Log

All actions from Android and web are logged automatically:
- Login / Logout
- Stock In / Out / Taking / Prep submissions
- Item and location CRUD
- Gate reader events
- Settings changes

Configure auto-delete under **Activity Log → Settings** (e.g., delete logs older than 30 days).

---

## Transaction History Export

Go to **Transaction History** and use the export buttons:

| Format | Library |
|--------|---------|
| Excel (.xlsx) | ClosedXML / EPPlus |
| PDF | iTextSharp / DinkToPdf |
| CSV | Built-in stream writer |

Filters available: date range, transaction type, location, status.

---

## Tech Stack

| Component | Technology |
|-----------|-----------|
| Framework | ASP.NET Core 8 MVC |
| Database | SQL Server |
| ORM | Entity Framework Core 8 |
| Authentication | JWT Bearer |
| Password hashing | BCrypt.Net |
| Frontend | Bootstrap 5 + DataTables + Chart.js |
| Testing | xUnit + Moq + FluentAssertions |

---

## Running Tests

```bash
cd InvenScan.Tests
dotnet test
```

Expected: **27 tests, 0 failures**

---

## License

This is a commercial product by **Zuxlabs**. All rights reserved.

Purchased licenses include:
- **Regular License** — use in a single end product
- **Extended License** — use in multiple end products or SaaS

---

## Support

- GitHub Issues: [invenscan-backend/issues](https://github.com/zuxua23/invenscan-backend/issues)
- Email: support@zuxlabs.dev
