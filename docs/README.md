# InvenScan — RFID / Barcode Inventory System

InvenScan is a commercial, hardware-agnostic inventory control system for
warehouses that use handheld RFID / barcode terminals (Zebra, Honeywell,
Denso, and similar). It ships as two projects:

| Project | Path | Stack |
|---------|------|-------|
| Backend + Web dashboard | `invenscan-backend/` | ASP.NET Core MVC 8, EF Core, SQL Server, JWT |
| Android handheld app | `invenscan-android/` | Kotlin, MVVM + Repository, Hilt, Room, Retrofit, WorkManager |

The headline feature is the **abstract `ScannerContract`** on Android: every
scanner interaction goes through one interface, so the app runs on any device
with a `MockScanner` out of the box and a buyer plugs in their own SDK without
touching feature code. See [`SCANNER_INTEGRATION.md`](SCANNER_INTEGRATION.md).

---

## Features

- **Stock In** — receive goods by RFID/barcode into a location, offline-capable.
- **Stock Taking** — open a counting session, scan, and reconcile found / missing.
- **Stock Preparation** — picking lists with scan-to-pick and quantity tracking.
- **Search Item** — scan a code and see item, location and status.
- **Web dashboard** — items, locations, sessions, picking lists, users.
- **Offline-first** — all handheld transactions queue in Room and sync via
  WorkManager when connectivity returns.

---

## Requirements

**Backend**
- .NET SDK 8.0+
- SQL Server 2017+ (or Azure SQL / SQL Server Express / LocalDB)

**Android**
- Android Studio (Koala / Ladybug or newer)
- JDK 17, Android SDK with API 34
- Min SDK 21, Target SDK 34

---

## Backend setup

```bash
cd invenscan-backend

# 1. Configure the database connection and a strong JWT secret.
#    Edit appsettings.json, OR (recommended) override via environment variables:
export ConnectionStrings__DefaultConnection="Server=localhost;Database=InvenScanDb;Trusted_Connection=True;TrustServerCertificate=True;"
export JwtSettings__SecretKey="<your-own-256-bit-secret>"

# 2. Restore + run. The schema is created and seeded automatically on startup.
dotnet restore
dotnet run
```

The API listens on the URLs in `Properties/launchSettings.json` (default
`https://localhost:7xxx` / `http://localhost:5xxx`). Health check: `GET /api/ping`.

> Manual DB provisioning is also possible with [`../sql/migration_script.sql`](../sql/migration_script.sql)
> followed by [`../sql/seed_data.sql`](../sql/seed_data.sql). See
> [`DATABASE_SETUP.md`](DATABASE_SETUP.md).

**Default credentials** (auto-seeded on first run):

| User | Password | Role |
|------|----------|------|
| `admin` | `admin123` | ADMIN |
| `operator1` | `operator123` | OPERATOR |

Change these before going to production.

---

## Android setup

```bash
cd invenscan-android
# Open in Android Studio and let Gradle sync, or:
./gradlew assembleDebug
```

There is **no hardcoded server URL**. On the login screen the operator enters:

- **Server URL** — e.g. `http://192.168.1.10:5000` (the machine running the backend)
- **Username / Password** — the seeded credentials above

The URL and JWT are stored in `EncryptedSharedPreferences`. Cleartext HTTP is
permitted by default for on-premise LAN servers (see the security notes); switch
to HTTPS for production.

To use real scanner hardware instead of `MockScanner`, follow
[`SCANNER_INTEGRATION.md`](SCANNER_INTEGRATION.md).

---

## End-to-end smoke test

1. Start the backend; confirm `GET /api/ping` returns `{ "success": true, ... }`.
2. Launch the Android app, enter the server URL + `admin` / `admin123`, log in.
3. **Stock In** → pick a location → scan (Mock) → Save → verify the document on
   the web dashboard.
4. **Stock Taking** → (create a session from the web/Postman first) → join →
   scan → Submit → verify found/missing on the web.
5. **Stock Prep** → open the sample picking list → scan to pick → Submit →
   verify picked quantities / status on the web.
6. **Search Item** → scan `ITM-001` (barcode) or a seeded EPC → see the detail.
7. **Offline**: stop the backend → Stock In / Stock Prep a few scans → Save
   (queued) → start the backend → the next WorkManager run (or app relaunch)
   syncs the queue.

---

## Documentation index

- [`API_DOCUMENTATION.md`](API_DOCUMENTATION.md) — every endpoint, request/response, error codes.
- [`SCANNER_INTEGRATION.md`](SCANNER_INTEGRATION.md) — implement `ScannerContract` for Zebra / Honeywell / Denso.
- [`DATABASE_SETUP.md`](DATABASE_SETUP.md) — schema, migrations, seeding.
- `../postman/InvenScan.postman_collection.json` — importable API collection.
- `../sql/` — standalone SQL schema + seed scripts.

---

## Security notes (read before production)

- **JWT secret**: replace the default in `appsettings.json` with a unique
  256-bit value supplied via environment variable / secret store.
- **Default users**: rotate `admin` / `operator1` passwords.
- **Transport**: serve the backend over HTTPS (TLS 1.2+). The Android
  `network_security_config.xml` permits cleartext for LAN use — set
  `cleartextTrafficPermitted="false"` and add certificate pinning for production.
- **CORS**: restrict `Cors:AllowedOrigins` in `appsettings.json` to your dashboard origin(s).
- Login is rate-limited (5 requests / minute / IP). Passwords are BCrypt-hashed.
