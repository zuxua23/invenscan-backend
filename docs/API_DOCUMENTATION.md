# InvenScan API Documentation

Base URL: `{{server}}/api` (e.g. `http://192.168.1.10:5000/api`).
All JSON is **camelCase** on the wire (request and response).

## Response envelope

Every endpoint (except `/api/ping`) returns this wrapper:

```json
{
  "success": true,
  "data": { },
  "message": "string",
  "errors": null
}
```

- `success` — `true` on 2xx business success, `false` otherwise.
- `data` — the payload (object, array, or `null`).
- `message` — human-readable status / error summary (generic, never a stack trace).
- `errors` — optional validation detail, usually `null`.

## Authentication

JWT Bearer. Obtain a token from `POST /api/auth/login`, then send it on every
other call:

```
Authorization: Bearer <token>
```

- Tokens are HS256, valid for `JwtSettings:ExpiryHours` (default 24h).
- API endpoints authenticate against the **Bearer** scheme; the web dashboard
  uses a separate cookie scheme.
- Role-restricted endpoints require the `ADMIN` role.

## Status & error codes

| Code | Meaning |
|------|---------|
| 200 | Success |
| 400 | Validation / business rule failure (`success:false`) |
| 401 | Missing/invalid/expired token |
| 403 | Authenticated but lacking the required role |
| 404 | Resource not found |
| 429 | Rate limit exceeded (login: 5/min/IP) |
| 500 | Unhandled server error (generic message only) |

---

## Health

### `GET /api/ping`  — _no auth_
```json
{ "success": true, "message": "InvenScan API is running", "timestamp": "2026-06-11T08:00:00Z" }
```

---

## Auth

### `POST /api/auth/login`  — _no auth, rate-limited 5/min/IP_
Request:
```json
{ "userId": "admin", "password": "admin123" }
```
Response `data`:
```json
{
  "token": "eyJhbGciOi...",
  "userId": "admin",
  "fullName": "Administrator",
  "role": "ADMIN",
  "expiresAt": "2026-06-12T08:00:00Z"
}
```
`401` with `success:false` on invalid credentials.

---

## Items  — _auth required_

### `GET /api/item`
`data`: array of
```json
{ "id": 1, "itemCode": "ITM-001", "itemName": "Laptop Dell Inspiron 15",
  "description": "Laptop 15 inch", "unit": "PCS", "minStock": 2,
  "createdAt": "2026-06-10T00:00:00Z" }
```

### `GET /api/item/{id}`
`data`: a single item object (as above), or `404`.

---

## Locations  — _auth required_

### `GET /api/location`
`data`: array of
```json
{ "id": 1, "locationCode": "LOC-001", "locationName": "Gudang Utama",
  "description": "Main warehouse", "createdAt": "2026-06-10T00:00:00Z" }
```

---

## Tags  — _auth required_

### `GET /api/tag/{identifier}`
`identifier` = EPC or TagId. `data`:
```json
{ "id": 1, "tagId": "TAG-0001", "epcTag": "E200...A1B1", "status": "IN_STOCK",
  "createdAt": "2026-06-10T00:00:00Z",
  "item": { "id": 1, "itemCode": "ITM-001", "itemName": "..." },
  "location": { "id": 1, "locationCode": "LOC-001", "locationName": "..." } }
```

### `POST /api/tag/register`  — _ADMIN_
```json
{ "tags": [ { "tagId": "TAG-0006", "epcTag": "E200...A1B6", "itemId": 1, "locationId": 1 } ] }
```

---

## Stock In  — _auth required_

### `GET /api/stockin?code={code}&scannerType={RFID|BARCODE}`
Resolve a scanned code to item info. `data`:
```json
{ "scannedCode": "E200...A1B1", "scanType": "RFID", "tagId": 1, "epcTag": "E200...A1B1",
  "itemId": 1, "itemCode": "ITM-001", "itemName": "Laptop Dell Inspiron 15",
  "unit": "PCS", "locationCode": "LOC-001", "locationName": "Gudang Utama",
  "tagStatus": "IN_STOCK" }
```

### `POST /api/stockin`
Submit a stock-in document. **The detail array is named `details`.**
```json
{
  "locationId": 1,
  "notes": "Received from supplier",
  "details": [
    { "itemId": 1, "scannedCode": "E200...A1B1", "scanType": "RFID" },
    { "itemId": 2, "scannedCode": "ITM-002",     "scanType": "BARCODE" }
  ]
}
```
`data`: the created document summary (`id`, `docNumber`, `locationName`,
`status`, `createdBy`, `createdAt`, `totalItems`).

### `POST /api/stockin/bulk-info`
```json
{ "codes": ["E200...A1B1", "E200...A1B2"], "scannerType": "RFID" }
```
`data`: array of the same shape as the lookup response.

---

## Stock Taking  — _auth required_

### `POST /api/stock-taking`  — _ADMIN_
```json
{ "remark": "Monthly stock check Q1 2026" }
```
Creates a session and snapshots all `IN_STOCK` tags as `SYSTEM` details.
Fails (`400`) if a session is already open.

### `GET /api/stock-taking`
`data`: array of session summaries (`id`, `sessionCode`, `remark`, `status`,
`createdBy`, `createdAt`, `closedAt`, `totalItems`, `scannedItems`, `missingItems`).

### `GET /api/stock-taking/active`
`data`: the open session summary, or `404` if none.

### `GET /api/stock-taking/tags/{sttId}`
`data`: array of session details
```json
{ "id": 10, "sttId": 1, "tagId": 5, "epcTag": "E200...A1B1", "itemId": 1,
  "itemCode": "ITM-001", "itemName": "...", "locationCode": "LOC-001",
  "locationName": "...", "action": "SYSTEM", "scannedAt": null }
```

### `GET /api/stock-taking/available-tags/{sttId}`
`data`: `IN_STOCK` tags not yet part of the session (Tag shape).

### `POST /api/stock-taking/operator-submit`
Submit handheld results. The detail array is named `scannedTags`; `tagId` is the
**session tag id** returned by `GET .../tags/{sttId}` (sent as a string). Session
details whose id appears with `action: "SCAN"` are marked scanned; the rest are
marked `MISSING`. The operator id is taken from the **JWT**, not the body.
```json
{
  "sttId": 1,
  "scannedTags": [
    { "tagId": "5", "itemId": 1, "action": "SCAN",    "scannedAt": 1749600000000 },
    { "tagId": "6", "itemId": 2, "action": "MISSING", "scannedAt": 1749600000000 }
  ]
}
```
`data`: the updated session summary (with refreshed `scannedItems` / `missingItems`).

---

## Stock Preparation (Picking)  — _auth required_

### `GET /api/stockprep`
`data`: array of open / in-progress picking lists (`id`, `docNumber`, `notes`,
`status`, `createdBy`, `createdAt`, `totalItems`, `pickedItems`).

### `GET /api/stockprep/{id}`
`data`: the picking list including `details`:
```json
{ "id": 1, "docNumber": "SP-SAMPLE-001", "status": "OPEN",
  "details": [
    { "id": 1, "itemId": 1, "itemCode": "ITM-001", "itemName": "...",
      "locationId": 1, "locationName": "...", "requestedQty": 1, "pickedQty": 0,
      "status": "PENDING", "scannedCode": null }
  ] }
```

### `POST /api/stockprep/bulk`
Submit picked items. The detail array is named `items`; `detailId` is the
picking-list detail `id`; `pickedQty` is the device-authoritative quantity.
```json
{
  "stockPrepId": 1,
  "items": [
    { "detailId": 1, "scannedCode": "ITM-001", "scanType": "BARCODE", "pickedQty": 1 }
  ]
}
```
A detail becomes `PICKED` when `pickedQty >= requestedQty`; the document becomes
`DONE` when all details are picked, else `IN_PROGRESS`. `data`: the refreshed list.

---

## Search Item  — _auth required_

### `GET /api/search-item`
`data`: array used to seed the handheld cache:
```json
{ "id": 1, "itemCode": "ITM-001", "itemName": "...", "description": "...",
  "unit": "PCS", "minStock": 2, "locationName": "Gudang Utama",
  "status": "IN_STOCK", "epcTag": "E200...A1B1" }
```
`locationName` / `status` / `epcTag` come from the item's representative tag
(null if the item has no registered tag).

### `GET /api/search-item/{code}`
`code` = item code or EPC. `data`: a single object as above, or `404`.

---

## Users  — _ADMIN only_

### `GET /api/user`
`data`: array of `{ id, userId, fullName, role, isActive, createdAt }`.

### `POST /api/user`
```json
{ "userId": "operator2", "fullName": "Operator Two", "password": "operator123", "role": "OPERATOR" }
```

### `PUT /api/user/{userId}`
```json
{ "fullName": "Operator Two", "role": "OPERATOR", "isActive": true, "newPassword": null }
```
`newPassword` is optional; omit/`null` to keep the existing password.
