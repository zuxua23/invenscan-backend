/* =====================================================================
   InvenScan — sample seed data
   ---------------------------------------------------------------------
   Run AFTER migration_script.sql.

   USERS: The application auto-seeds the default users (admin / operator1)
   on first launch via AppDbSeeder, hashing passwords with BCrypt at
   runtime. Seeding users from raw SQL requires a precomputed BCrypt hash
   (cost >= 12) — see the note at the bottom of this file. By default this
   script does NOT insert users so it stays compatible with the app
   seeder; let the application create them, then sign in with:

       admin / admin123        (ADMIN)
       operator1 / operator123 (OPERATOR)

   The blocks below seed master data (items, locations), RFID tags, and a
   sample picking list so the handheld flows have data to work with.
   All inserts are idempotent (guarded by NOT EXISTS).
   ===================================================================== */

USE [InvenScanDb];
GO

/* ---------- Items ---------- */
INSERT INTO [dbo].[tb_Item] ([ItemCode],[ItemName],[Description],[Unit],[MinStock],[CreatedBy],[CreatedAt],[IsDelete])
SELECT v.ItemCode, v.ItemName, v.Description, v.Unit, v.MinStock, 'admin', SYSUTCDATETIME(), 0
FROM (VALUES
    ('ITM-001', 'Laptop Dell Inspiron 15',        'Laptop 15 inch',                'PCS', 2),
    ('ITM-002', 'Mouse Wireless Logitech M185',   'Wireless mouse',                'PCS', 5),
    ('ITM-003', 'Keyboard USB Mechanical',        'Mechanical keyboard USB',       'PCS', 3),
    ('ITM-004', 'Monitor LED 24 inch',            'LED Monitor Full HD',           'PCS', 2),
    ('ITM-005', 'UPS 650VA',                      'Uninterruptible Power Supply',  'PCS', 1)
) AS v(ItemCode, ItemName, Description, Unit, MinStock)
WHERE NOT EXISTS (SELECT 1 FROM [dbo].[tb_Item] i WHERE i.ItemCode = v.ItemCode);
GO

/* ---------- Locations ---------- */
INSERT INTO [dbo].[tb_Location] ([LocationCode],[LocationName],[Description],[CreatedBy],[CreatedAt],[IsDelete])
SELECT v.LocationCode, v.LocationName, v.Description, 'admin', SYSUTCDATETIME(), 0
FROM (VALUES
    ('LOC-001', 'Gudang Utama',         'Main warehouse'),
    ('LOC-002', 'Ruang IT',             'IT room storage'),
    ('LOC-003', 'Lantai 2 - Storage',   'Second floor storage')
) AS v(LocationCode, LocationName, Description)
WHERE NOT EXISTS (SELECT 1 FROM [dbo].[tb_Location] l WHERE l.LocationCode = v.LocationCode);
GO

/* ---------- RFID Tags (EPC -> item/location), needed for RFID stock-in,
   stock-taking and search-by-EPC demos) ---------- */
INSERT INTO [dbo].[tb_Tag] ([TagId],[EpcTag],[ItemId],[LocationId],[Status],[CreatedAt])
SELECT v.TagId, v.EpcTag, i.Id, l.Id, 'IN_STOCK', SYSUTCDATETIME()
FROM (VALUES
    ('TAG-0001', 'E2000017221101441890A1B1', 'ITM-001', 'LOC-001'),
    ('TAG-0002', 'E2000017221101441890A1B2', 'ITM-002', 'LOC-001'),
    ('TAG-0003', 'E2000017221101441890A1B3', 'ITM-003', 'LOC-002'),
    ('TAG-0004', 'E2000017221101441890A1B4', 'ITM-004', 'LOC-002'),
    ('TAG-0005', 'E2000017221101441890A1B5', 'ITM-005', 'LOC-003')
) AS v(TagId, EpcTag, ItemCode, LocationCode)
JOIN [dbo].[tb_Item] i     ON i.ItemCode = v.ItemCode
JOIN [dbo].[tb_Location] l ON l.LocationCode = v.LocationCode
WHERE NOT EXISTS (SELECT 1 FROM [dbo].[tb_Tag] t WHERE t.TagId = v.TagId);
GO

/* ---------- Sample picking list (StockPrep) for the handheld pick flow ---------- */
IF NOT EXISTS (SELECT 1 FROM [dbo].[tb_StockPrep] WHERE [DocNumber] = 'SP-SAMPLE-001')
BEGIN
    INSERT INTO [dbo].[tb_StockPrep] ([DocNumber],[Notes],[Status],[CreatedBy],[CreatedAt])
    VALUES ('SP-SAMPLE-001', 'Sample picking list for demo', 'OPEN', 'admin', SYSUTCDATETIME());

    DECLARE @prepId INT = SCOPE_IDENTITY();

    INSERT INTO [dbo].[tb_StockPrepDetail]
        ([StockPrepId],[ItemId],[LocationId],[RequestedQty],[PickedQty],[Status],[ScannedCode],[CreatedBy])
    SELECT @prepId, i.Id, l.Id, v.RequestedQty, 0, 'PENDING', '', 'admin'
    FROM (VALUES
        ('ITM-001', 'LOC-001', 1),
        ('ITM-002', 'LOC-001', 2),
        ('ITM-003', 'LOC-002', 1)
    ) AS v(ItemCode, LocationCode, RequestedQty)
    JOIN [dbo].[tb_Item] i     ON i.ItemCode = v.ItemCode
    JOIN [dbo].[tb_Location] l ON l.LocationCode = v.LocationCode;
END
GO

/* =====================================================================
   OPTIONAL — seed users from SQL instead of the app seeder.
   Generate a BCrypt hash (cost 12) for each password and paste it below.

     C#:   BCrypt.Net.BCrypt.HashPassword("admin123", 12)
     CLI:  htpasswd -bnBC 12 "" admin123 | tr -d ':\n' | sed 's/$2y/$2a/'

   Then uncomment:

   -- INSERT INTO [dbo].[tb_User] ([UserId],[FullName],[PasswordHash],[Role],[IsActive],[CreatedAt])
   -- SELECT 'admin', 'Administrator', '<BCRYPT_HASH_FOR_admin123>', 'ADMIN', 1, SYSUTCDATETIME()
   -- WHERE NOT EXISTS (SELECT 1 FROM [dbo].[tb_User] WHERE [UserId] = 'admin');

   -- INSERT INTO [dbo].[tb_User] ([UserId],[FullName],[PasswordHash],[Role],[IsActive],[CreatedAt])
   -- SELECT 'operator1', 'Operator One', '<BCRYPT_HASH_FOR_operator123>', 'OPERATOR', 1, SYSUTCDATETIME()
   -- WHERE NOT EXISTS (SELECT 1 FROM [dbo].[tb_User] WHERE [UserId] = 'operator1');
   ===================================================================== */
