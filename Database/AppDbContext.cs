using InvenScan.Entity;
using Microsoft.EntityFrameworkCore;

namespace InvenScan.Database;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<Item> Items => Set<Item>();
    public DbSet<Location> Locations => Set<Location>();
    public DbSet<Tag> Tags => Set<Tag>();
    public DbSet<StockTaking> StockTakings => Set<StockTaking>();
    public DbSet<StockTakingDetail> StockTakingDetails => Set<StockTakingDetail>();
    public DbSet<StockIn> StockIns => Set<StockIn>();
    public DbSet<StockInDetail> StockInDetails => Set<StockInDetail>();
    public DbSet<StockPrep> StockPreps => Set<StockPrep>();
    public DbSet<StockPrepDetail> StockPrepDetails => Set<StockPrepDetail>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("tb_User");
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.UserId).IsUnique();
            entity.Property(e => e.UserId).HasMaxLength(50).IsRequired();
            entity.Property(e => e.FullName).HasMaxLength(100).IsRequired();
            entity.Property(e => e.PasswordHash).IsRequired();
            entity.Property(e => e.Role).HasMaxLength(20).IsRequired();
        });

        modelBuilder.Entity<Item>(entity =>
        {
            entity.ToTable("tb_Item");
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.ItemCode).IsUnique();
            entity.Property(e => e.ItemCode).HasMaxLength(50).IsRequired();
            entity.Property(e => e.ItemName).HasMaxLength(200).IsRequired();
            entity.Property(e => e.Unit).HasMaxLength(20);
        });

        modelBuilder.Entity<Location>(entity =>
        {
            entity.ToTable("tb_Location");
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.LocationCode).IsUnique();
            entity.Property(e => e.LocationCode).HasMaxLength(50).IsRequired();
            entity.Property(e => e.LocationName).HasMaxLength(200).IsRequired();
        });

        modelBuilder.Entity<Tag>(entity =>
        {
            entity.ToTable("tb_Tag");
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.TagId).IsUnique();
            entity.HasIndex(e => e.EpcTag).IsUnique();
            entity.Property(e => e.TagId).HasMaxLength(100).IsRequired();
            entity.Property(e => e.EpcTag).HasMaxLength(100).IsRequired();
            entity.Property(e => e.Status).HasMaxLength(20);
            entity.HasOne(e => e.Item).WithMany(i => i.Tags).HasForeignKey(e => e.ItemId);
            entity.HasOne(e => e.Location).WithMany(l => l.Tags).HasForeignKey(e => e.LocationId);
        });

        modelBuilder.Entity<StockTaking>(entity =>
        {
            entity.ToTable("tb_StockTaking");
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.SessionCode).IsUnique();
            entity.Property(e => e.SessionCode).HasMaxLength(50).IsRequired();
            entity.Property(e => e.Status).HasMaxLength(20);
        });

        modelBuilder.Entity<StockTakingDetail>(entity =>
        {
            entity.ToTable("tb_StockTakingDetail");
            entity.HasKey(e => e.Id);
            entity.HasOne(e => e.StockTaking).WithMany(s => s.Details).HasForeignKey(e => e.SttId);
            entity.HasOne(e => e.Tag).WithMany().HasForeignKey(e => e.TagId);
            entity.HasOne(e => e.Item).WithMany().HasForeignKey(e => e.ItemId);
            entity.Property(e => e.Action).HasMaxLength(20);
        });

        modelBuilder.Entity<StockIn>(entity =>
        {
            entity.ToTable("tb_StockIn");
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.DocNumber).IsUnique();
            entity.Property(e => e.DocNumber).HasMaxLength(50).IsRequired();
            entity.Property(e => e.Status).HasMaxLength(20);
            entity.HasOne(e => e.Location).WithMany(l => l.StockIns).HasForeignKey(e => e.LocationId);
        });

        modelBuilder.Entity<StockInDetail>(entity =>
        {
            entity.ToTable("tb_StockInDetail");
            entity.HasKey(e => e.Id);
            entity.HasOne(e => e.StockIn).WithMany(s => s.Details).HasForeignKey(e => e.StockInId);
            entity.HasOne(e => e.Tag).WithMany().HasForeignKey(e => e.TagId).IsRequired(false);
            entity.HasOne(e => e.Item).WithMany().HasForeignKey(e => e.ItemId);
            entity.Property(e => e.ScanType).HasMaxLength(20);
        });

        modelBuilder.Entity<StockPrep>(entity =>
        {
            entity.ToTable("tb_StockPrep");
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.DocNumber).IsUnique();
            entity.Property(e => e.DocNumber).HasMaxLength(50).IsRequired();
            entity.Property(e => e.Status).HasMaxLength(20);
        });

        modelBuilder.Entity<StockPrepDetail>(entity =>
        {
            entity.ToTable("tb_StockPrepDetail");
            entity.HasKey(e => e.Id);
            entity.HasOne(e => e.StockPrep).WithMany(s => s.Details).HasForeignKey(e => e.StockPrepId);
            entity.HasOne(e => e.Item).WithMany().HasForeignKey(e => e.ItemId);
            entity.HasOne(e => e.Location).WithMany().HasForeignKey(e => e.LocationId);
            entity.Property(e => e.Status).HasMaxLength(20);
        });
    }
}
