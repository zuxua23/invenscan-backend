# Screenshots

This folder holds the marketing/preview screenshots that ship with the product
listing. They must be captured from a **running** instance (a real device/
emulator and a browser), so they are not generated as part of the source tree.

Capture the following at the resolutions below and drop the PNGs into the
matching subfolder.

## `android/` (1080×1920 portrait, handheld or emulator with the seed data loaded)

| File | Screen |
|------|--------|
| `01_login.png` | Login (server URL + credentials) |
| `02_home.png` | Home dashboard grid |
| `03_stock_in.png` | Stock In with a few scanned items |
| `04_stock_taking.png` | Stock Taking session / scan counters |
| `05_stock_prep.png` | Stock Prep picking list + detail |
| `06_search_item.png` | Search Item result detail |
| `07_settings.png` | Settings (server URL, device id, scanner type) |

Capture tip: `adb exec-out screencap -p > android/03_stock_in.png`.

## `web/` (1440×900, dashboard logged in as `admin`)

| File | Screen |
|------|--------|
| `01_dashboard.png` | Dashboard summary cards |
| `02_items.png` | Items list (DataTables) |
| `03_locations.png` | Locations list |
| `04_stock_taking.png` | Stock taking sessions + detail |
| `05_stock_prep.png` | Picking list create + detail |
| `06_users.png` | User management |

> Note: screenshots are intentionally not committed as binaries here. Generate
> them against the seeded demo data (`sql/seed_data.sql`) for consistent,
> reproducible previews before publishing the listing.
