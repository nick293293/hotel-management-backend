using BackendApi.Models;
using Microsoft.EntityFrameworkCore;

namespace BackendApi.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Ping> Ping { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            var e = modelBuilder.Entity<Ping>();
            e.ToTable("ping");
            e.HasKey(p => p.Id);
            e.Property(p => p.Id).HasColumnName("id");
            e.Property(p => p.Note).HasColumnName("note").HasMaxLength(100).IsRequired();
            e.Property(p => p.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("CURRENT_TIMESTAMP");
        }
    }
}
