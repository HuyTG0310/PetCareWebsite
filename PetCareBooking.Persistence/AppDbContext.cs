using Microsoft.EntityFrameworkCore;
using PetCareBooking.Domain.Entities;
using PetCareBooking.Domain.Enums;

namespace PetCareBooking.Persistence;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    // --- PHáº¦N 1: RBAC & NHÃ‚N Sá»° ---
    public DbSet<Permission> Permissions { get; set; }
    public DbSet<Role> Roles { get; set; }
    public DbSet<RolePermission> RolePermissions { get; set; }
    public DbSet<Staff> Staffs { get; set; }
    public DbSet<StaffRole> StaffRoles { get; set; }

    // --- PHáº¦N 2: KHÃCH HÃ€NG & THÃš CÆ¯NG ---
    public DbSet<Customer> Customers { get; set; }
    public DbSet<Pet> Pets { get; set; }

    // --- PHáº¦N 3: Dá»ŠCH Vá»¤ & PHÃ’NG ---
    public DbSet<Service> Services { get; set; }
    public DbSet<ServicePrice> ServicePrices { get; set; }
    public DbSet<RoomType> RoomTypes { get; set; }
    public DbSet<Room> Rooms { get; set; }
    public DbSet<Promotion> Promotions { get; set; }

    // --- PHáº¦N 4: Váº¬N HÃ€NH (SHOPPING CART) ---
    public DbSet<Booking> Bookings { get; set; }
    public DbSet<BookingItem> BookingItems { get; set; }
    public DbSet<CareRecord> CareRecords { get; set; }
    public DbSet<Review> Reviews { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // ==========================================
        // PHáº¦N 1: Há»† THá»NG PHÃ‚N QUYá»€N (RBAC) & NHÃ‚N Sá»°
        // ==========================================

        modelBuilder.Entity<Permission>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasDefaultValueSql("NEWSEQUENTIALID()");
            entity.Property(e => e.PermissionCode).HasMaxLength(100).IsRequired();
            entity.HasIndex(e => e.PermissionCode).IsUnique();
            entity.Property(e => e.PermissionName).HasMaxLength(150).IsRequired();
            entity.Property(e => e.ModuleName).HasMaxLength(50).IsRequired();
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasDefaultValueSql("NEWSEQUENTIALID()");
            entity.Property(e => e.RoleName).HasMaxLength(100).IsRequired();
            entity.HasIndex(e => e.RoleName).IsUnique();
            entity.Property(e => e.Description).HasMaxLength(255);
        });

        modelBuilder.Entity<RolePermission>(entity =>
        {
            entity.HasKey(e => new { e.RoleId, e.PermissionId });

            entity.HasOne(e => e.Role)
                  .WithMany(r => r.RolePermissions)
                  .HasForeignKey(e => e.RoleId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Permission)
                  .WithMany(p => p.RolePermissions)
                  .HasForeignKey(e => e.PermissionId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Staff>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasDefaultValueSql("NEWSEQUENTIALID()");
            entity.Property(e => e.Email).HasMaxLength(100).IsRequired();
            entity.HasIndex(e => e.Email).IsUnique();
            entity.Property(e => e.PasswordHash).HasMaxLength(255).IsRequired();
            entity.Property(e => e.FullName).HasMaxLength(100).IsRequired();
            entity.Property(e => e.PhoneNumber).HasMaxLength(20).IsRequired();
            entity.Property(e => e.YearsOfExperience).HasDefaultValue(0);

            entity.Property(e => e.Status)
                  .HasMaxLength(20)
                  .HasConversion<string>()
                  .HasDefaultValue(StaffStatus.Active);
        });

        modelBuilder.Entity<StaffRole>(entity =>
        {
            entity.HasKey(e => new { e.StaffId, e.RoleId });

            entity.HasOne(e => e.Staff)
                  .WithMany(s => s.StaffRoles)
                  .HasForeignKey(e => e.StaffId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Role)
                  .WithMany(r => r.StaffRoles)
                  .HasForeignKey(e => e.RoleId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // ==========================================
        // PHáº¦N 2: KHÃCH HÃ€NG & THÃš CÆ¯NG
        // ==========================================

        modelBuilder.Entity<Customer>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasDefaultValueSql("NEWSEQUENTIALID()");
            entity.Property(e => e.Email).HasMaxLength(100).IsRequired();
            entity.HasIndex(e => e.Email).IsUnique();
            entity.Property(e => e.PasswordHash).HasMaxLength(255).IsRequired();
            entity.Property(e => e.FullName).HasMaxLength(100).IsRequired();
            entity.Property(e => e.PhoneNumber).HasMaxLength(20).IsRequired();
            entity.Property(e => e.Address).HasMaxLength(255);
            entity.Property(e => e.CreatedAt).HasColumnType("DATETIME").HasDefaultValueSql("GETDATE()");
            entity.Property(e => e.IsActive).HasDefaultValue(true);
        });

        modelBuilder.Entity<Pet>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasDefaultValueSql("NEWSEQUENTIALID()");
            entity.Property(e => e.Name).HasMaxLength(100).IsRequired();
            entity.Property(e => e.Breed).HasMaxLength(100);
            entity.Property(e => e.Weight).HasColumnType("DECIMAL(5,2)").IsRequired();
            entity.Property(e => e.HealthNotes).HasColumnType("NVARCHAR(MAX)");
            entity.Property(e => e.IsActive).HasDefaultValue(true);

            entity.Property(e => e.Species)
                  .HasMaxLength(20)
                  .HasConversion<string>()
                  .IsRequired();

            entity.HasOne(e => e.Owner)
                  .WithMany(c => c.Pets)
                  .HasForeignKey(e => e.CustomerId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // ==========================================
        // PHáº¦N 3: Cáº¤U HÃŒNH Dá»ŠCH Vá»¤, PHÃ’NG & KHUYáº¾N MÃƒI
        // ==========================================

        modelBuilder.Entity<Service>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasDefaultValueSql("NEWSEQUENTIALID()");
            entity.Property(e => e.Name).HasMaxLength(150).IsRequired();
            entity.Property(e => e.Description).HasColumnType("NVARCHAR(MAX)");

            entity.Property(e => e.ServiceType)
                  .HasMaxLength(20)
                  .HasConversion<string>()
                  .IsRequired();

            entity.Property(e => e.IsActive).HasDefaultValue(true);
        });

        modelBuilder.Entity<ServicePrice>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasDefaultValueSql("NEWSEQUENTIALID()");
            entity.Property(e => e.MinWeight).HasColumnType("DECIMAL(5,2)");
            entity.Property(e => e.MaxWeight).HasColumnType("DECIMAL(5,2)");
            entity.Property(e => e.Price).HasColumnType("DECIMAL(18,2)").IsRequired();

            entity.Property(e => e.PricingUnit)
                  .HasMaxLength(20)
                  .HasConversion<string>()
                  .HasDefaultValue(PricingUnit.Per_Turn);

            entity.HasOne(e => e.Service)
                  .WithMany(s => s.ServicePrices)
                  .HasForeignKey(e => e.ServiceId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<RoomType>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasDefaultValueSql("NEWSEQUENTIALID()");
            entity.Property(e => e.Name).HasMaxLength(100).IsRequired();
            entity.Property(e => e.Description).HasColumnType("NVARCHAR(MAX)");
        });

        modelBuilder.Entity<Room>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasDefaultValueSql("NEWSEQUENTIALID()");
            entity.Property(e => e.RoomName).HasMaxLength(50).IsRequired();

            entity.Property(e => e.Status)
                  .HasMaxLength(20)
                  .HasConversion<string>()
                  .HasDefaultValue(RoomStatus.Available);

            entity.HasOne(e => e.RoomType)
                  .WithMany(rt => rt.Rooms)
                  .HasForeignKey(e => e.RoomTypeId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Promotion>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasDefaultValueSql("NEWSEQUENTIALID()");
            entity.Property(e => e.Code).HasMaxLength(50).IsRequired();
            entity.HasIndex(e => e.Code).IsUnique();

            entity.Property(e => e.DiscountType)
                  .HasMaxLength(20)
                  .HasConversion<string>()
                  .IsRequired();

            entity.Property(e => e.DiscountValue).HasColumnType("DECIMAL(18,2)").IsRequired();
            entity.Property(e => e.StartDate).HasColumnType("DATETIME").IsRequired();
            entity.Property(e => e.EndDate).HasColumnType("DATETIME").IsRequired();
            entity.Property(e => e.CurrentUsage).HasDefaultValue(0);
        });

        // ==========================================
        // PHáº¦N 4: Váº¬N HÃ€NH Dá»ŠCH Vá»¤ (SHOPPING CART)
        // ==========================================

        modelBuilder.Entity<Booking>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasDefaultValueSql("NEWSEQUENTIALID()");
            entity.Property(e => e.TotalPrice).HasColumnType("DECIMAL(18,2)").IsRequired();
            entity.Property(e => e.CreatedAt).HasColumnType("DATETIME").HasDefaultValueSql("GETDATE()");

            entity.Property(e => e.Status)
                  .HasMaxLength(20)
                  .HasConversion<string>()
                  .HasDefaultValue(BookingStatus.Pending);

            entity.Property(e => e.CancellationReason)
                  .HasMaxLength(500);

            entity.HasOne(e => e.Customer)
                  .WithMany()
                  .HasForeignKey(e => e.CustomerId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Promotion)
                  .WithMany()
                  .HasForeignKey(e => e.PromotionId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<BookingItem>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasDefaultValueSql("NEWSEQUENTIALID()");

            entity.Property(e => e.ScheduledStartAt).HasColumnType("DATETIME").IsRequired();
            entity.Property(e => e.ScheduledEndAt).HasColumnType("DATETIME");
            entity.Property(e => e.Quantity).HasColumnType("DECIMAL(10,2)").HasDefaultValue(1m);
            entity.Property(e => e.UnitPrice).HasColumnType("DECIMAL(18,2)").IsRequired();
            entity.Property(e => e.AssignedPrice).HasColumnType("DECIMAL(18,2)").IsRequired();

            entity.Property(e => e.Status)
                  .HasMaxLength(20)
                  .HasConversion<string>()
                  .HasDefaultValue(BookingItemStatus.Pending);

            entity.Property(e => e.ResultNote).HasColumnType("NVARCHAR(MAX)");
            entity.Property(e => e.ResultImageUrl).HasColumnType("VARCHAR(MAX)");

            entity.HasOne(e => e.Booking)
                  .WithMany(b => b.BookingItems)
                  .HasForeignKey(e => e.BookingId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Pet)
                  .WithMany()
                  .HasForeignKey(e => e.PetId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Service)
                  .WithMany()
                  .HasForeignKey(e => e.ServiceId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Room)
                  .WithMany()
                  .HasForeignKey(e => e.RoomId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Staff)
                  .WithMany()
                  .HasForeignKey(e => e.StaffId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<CareRecord>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasDefaultValueSql("NEWSEQUENTIALID()");
            entity.Property(e => e.RecordDate).HasColumnType("DATETIME").HasDefaultValueSql("GETDATE()");
            entity.Property(e => e.HealthStatus).HasMaxLength(100);
            entity.Property(e => e.Note).HasColumnType("NVARCHAR(MAX)");
            entity.Property(e => e.ImageUrl).HasColumnType("VARCHAR(MAX)");

            entity.HasOne(e => e.BookingItem)
                  .WithMany(bi => bi.CareRecords)
                  .HasForeignKey(e => e.BookingItemId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Staff)
                  .WithMany()
                  .HasForeignKey(e => e.StaffId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Review>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasDefaultValueSql("NEWSEQUENTIALID()");

            // Äáº£m báº£o má»—i BookingItem chá»‰ Ä‘Æ°á»£c review 1 láº§n
            entity.HasIndex(e => e.BookingItemId).IsUnique();

            // RÃ ng buá»™c má»©c Ä‘á»™ Ä‘Ã¡nh giÃ¡ tá»« 1 Ä‘áº¿n 5
            entity.ToTable(t => t.HasCheckConstraint("CK_Review_Rating", "Rating >= 1 AND Rating <= 5"));

            entity.Property(e => e.CreatedAt).HasColumnType("DATETIME").HasDefaultValueSql("GETDATE()");

            entity.HasOne(e => e.Customer)
                  .WithMany()
                  .HasForeignKey(e => e.CustomerId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Service)
                  .WithMany()
                  .HasForeignKey(e => e.ServiceId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.BookingItem)
                  .WithOne(bi => bi.Review)
                  .HasForeignKey<Review>(e => e.BookingItemId)
                  .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
