# Database Setup

InvenScan uses **SQL Server** via **Entity Framework Core 8**. Schema and seed
data are applied automatically, with standalone SQL scripts available for
DBA-driven provisioning.

## Connection string

Set `ConnectionStrings:DefaultConnection` in `appsettings.json`, or override with
the `ConnectionStrings__DefaultConnection` environment variable.

```
Server=localhost;Database=InvenScanDb;Trusted_Connection=True;TrustServerCertificate=True;
```

SQL auth example:
```
Server=localhost;Database=InvenScanDb;User Id=sa;Password=Your_Pwd;TrustServerCertificate=True;
```

## Option A — automatic (recommended)

On startup `Program.cs` runs `context.Database.Migrate()` (applies the EF
migration, creating the database if needed) and then `AppDbSeeder.Seed(...)`,
which inserts the default users, 5 items, and 3 locations **only if those tables
are empty**.

```bash
dotnet run
```

To re-create the EF migration from scratch:
```bash
dotnet ef migrations add InitialCreate
dotnet ef database update
```

## Option B — manual SQL

Run against your SQL Server, in order:

1. `sql/migration_script.sql` — creates `InvenScanDb`, all tables, indexes, and
   registers the migration in `__EFMigrationsHistory` so the app treats the
   schema as already applied.
2. `sql/seed_data.sql` — items, locations, RFID tags, and a sample picking list.
   Users are left to the app seeder by default (BCrypt hashes are generated at
   runtime); see the note at the bottom of that script to seed users via SQL.

## Schema overview

| Table | Purpose |
|-------|---------|
| `tb_User` | Accounts + role (ADMIN/OPERATOR), BCrypt password hash |
| `tb_Item` | Item master data (unique `ItemCode`, soft-delete) |
| `tb_Location` | Location master data (unique `LocationCode`, soft-delete) |
| `tb_Tag` | RFID tags: EPC → item + location + status (unique `TagId`, `EpcTag`) |
| `tb_StockIn` / `tb_StockInDetail` | Goods receipt headers + scanned lines |
| `tb_StockTaking` / `tb_StockTakingDetail` | Counting sessions + per-tag results |
| `tb_StockPrep` / `tb_StockPrepDetail` | Picking lists + requested/picked lines |

Master data (items, locations, tags) is server-authoritative; handheld
transactions are device-authoritative and sync from the app's Room queues.

## Default credentials

| User | Password | Role |
|------|----------|------|
| `admin` | `admin123` | ADMIN |
| `operator1` | `operator123` | OPERATOR |

Rotate these before production.
