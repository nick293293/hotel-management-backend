using BackendApi.Models;
using Microsoft.EntityFrameworkCore;

namespace BackendApi.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        // existing Ping
        public DbSet<Ping> Ping { get; set; } = null!;

        // new tables
        public DbSet<User> Users { get; set; } = null!;
        public DbSet<Room> Rooms { get; set; } = null!;
        public DbSet<Reservation> Reservations { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // --- Ping table ---
            var p = modelBuilder.Entity<Ping>();
            p.ToTable("ping");
            p.HasKey(x => x.Id);
            p.Property(x => x.Id).HasColumnName("id");
            p.Property(x => x.Note).HasColumnName("note").HasMaxLength(100).IsRequired();
            p.Property(x => x.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("CURRENT_TIMESTAMP");

            // --- Users table ---
            var u = modelBuilder.Entity<User>();
            u.ToTable("users");
            u.HasKey(x => x.UserId);
            u.Property(x => x.UserId).HasColumnName("user_id");
            u.Property(x => x.PasswordHashed).HasColumnName("password").HasMaxLength(255).IsRequired(); // <-- fixed
            u.Property(x => x.FirstName).HasColumnName("first_name").HasMaxLength(50).IsRequired();
            u.Property(x => x.LastName).HasColumnName("last_name").HasMaxLength(50).IsRequired();
            u.Property(x => x.Role).HasColumnName("role").HasMaxLength(50).IsRequired();
            u.Property(x => x.Email).HasColumnName("email").HasMaxLength(255).IsRequired();
            u.Property(x => x.PhoneNumber).HasColumnName("phone_number").HasMaxLength(20);
            u.Property(x => x.Address).HasColumnName("address").HasMaxLength(255);
            u.Property(x => x.CreatedAt).HasColumnName("created_at");
            u.Property(x => x.UpdatedAt).HasColumnName("updated_at");

            // --- Rooms table ---
            var r = modelBuilder.Entity<Room>();
            r.ToTable("rooms");
            r.HasKey(x => x.RoomId);
            r.Property(x => x.RoomId).HasColumnName("room_id");
            r.Property(x => x.RoomType).HasColumnName("room_type").HasMaxLength(50).IsRequired();
            r.Property(x => x.Price).HasColumnName("price").HasColumnType("decimal(10,2)");
            r.Property(x => x.Status).HasColumnName("status").HasMaxLength(50).IsRequired();
            r.Property(x => x.CreatedAt).HasColumnName("created_at");
            r.Property(x => x.UpdatedAt).HasColumnName("updated_at");

            // --- Reservations table ---
            var res = modelBuilder.Entity<Reservation>();
            res.ToTable("reservations");
            res.HasKey(x => x.ReservationId);
            res.Property(x => x.ReservationId).HasColumnName("reservation_id");
            res.Property(x => x.UserId).HasColumnName("user_id");
            res.Property(x => x.RoomId).HasColumnName("room_id");
            res.Property(x => x.CheckInDate).HasColumnName("check_in_date");
            res.Property(x => x.CheckOutDate).HasColumnName("check_out_date");
            res.Property(x => x.TotalAmount).HasColumnName("total_amount").HasColumnType("decimal(10,2)");
            res.Property(x => x.Status).HasColumnName("status").HasMaxLength(50).IsRequired();
            res.Property(x => x.CreatedAt).HasColumnName("created_at");
            res.Property(x => x.UpdatedAt).HasColumnName("updated_at");

            // --- Relationships ---
            res.HasOne(x => x.User)
               .WithMany(u => u.Reservations)
               .HasForeignKey(x => x.UserId)
               .HasConstraintName("fk_reservations_user");

            res.HasOne(x => x.Room)
               .WithMany(r => r.Reservations)
               .HasForeignKey(x => x.RoomId)
               .HasConstraintName("fk_reservations_room");
        }
    }
}
