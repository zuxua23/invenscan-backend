using InvenScan.Entity;

namespace InvenScan.Database;

public static class AppDbSeeder
{
    public static void Seed(AppDbContext context)
    {
        if (!context.Users.Any())
        {
            context.Users.AddRange(
                new User
                {
                    UserId = "admin",
                    FullName = "Administrator",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("admin123"),
                    Role = "ADMIN",
                    IsActive = true
                },
                new User
                {
                    UserId = "operator1",
                    FullName = "Operator One",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("operator123"),
                    Role = "OPERATOR",
                    IsActive = true
                }
            );
        }

        if (!context.Items.Any())
        {
            context.Items.AddRange(
                new Item { ItemCode = "ITM-001", ItemName = "Laptop Dell Inspiron 15", Description = "Laptop 15 inch", Unit = "PCS", MinStock = 2, CreatedBy = "admin" },
                new Item { ItemCode = "ITM-002", ItemName = "Mouse Wireless Logitech M185", Description = "Wireless mouse", Unit = "PCS", MinStock = 5, CreatedBy = "admin" },
                new Item { ItemCode = "ITM-003", ItemName = "Keyboard USB Mechanical", Description = "Mechanical keyboard USB", Unit = "PCS", MinStock = 3, CreatedBy = "admin" },
                new Item { ItemCode = "ITM-004", ItemName = "Monitor LED 24 inch", Description = "LED Monitor Full HD", Unit = "PCS", MinStock = 2, CreatedBy = "admin" },
                new Item { ItemCode = "ITM-005", ItemName = "UPS 650VA", Description = "Uninterruptible Power Supply", Unit = "PCS", MinStock = 1, CreatedBy = "admin" }
            );
        }

        if (!context.Locations.Any())
        {
            context.Locations.AddRange(
                new Location { LocationCode = "LOC-001", LocationName = "Gudang Utama", Description = "Main warehouse", CreatedBy = "admin" },
                new Location { LocationCode = "LOC-002", LocationName = "Ruang IT", Description = "IT room storage", CreatedBy = "admin" },
                new Location { LocationCode = "LOC-003", LocationName = "Lantai 2 - Storage", Description = "Second floor storage", CreatedBy = "admin" }
            );
        }

        context.SaveChanges();
    }
}
