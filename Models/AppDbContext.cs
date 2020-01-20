using Microsoft.EntityFrameworkCore;

namespace taesa_aprovador_api.Models
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        { 
        }

        public DbSet<User> Users {get; set;}
        public DbSet<Item> Items {get; set;}
        public DbSet<Notification> Notifications {get; set;}

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder); 

            // User
            modelBuilder.Entity<User>(entity => {
                entity.HasIndex(u => u.Email).IsUnique();
            });    

            modelBuilder.Entity<User>()
                .Property(u => u.CreatedAt)
                .HasDefaultValueSql("getdate()");

            modelBuilder.Entity<User>()
                .Property(u => u.UpdateAt)
                .HasDefaultValueSql("getdate()");

            // Notification
            modelBuilder.Entity<Notification>(entity => {
                entity.HasIndex(n => n.Uuid).IsUnique();
            });

            modelBuilder.Entity<Notification>()
                .Property(n => n.UserId)
                .IsRequired();

            modelBuilder.Entity<Notification>()
                .Property(n => n.CreatedAt)
                .HasDefaultValueSql("getdate()");

            modelBuilder.Entity<Notification>()
                .Property(n => n.UpdateAt)
                .HasDefaultValueSql("getdate()");                

            // Item
            modelBuilder.Entity<Item>()
                .HasIndex(i => i.TaesaId)
                .IsUnique();

            modelBuilder.Entity<Item>()
                .Property(i => i.UserId)
                .IsRequired();

            modelBuilder.Entity<Item>()
                .Property(i => i.DateLimit)
                .HasDefaultValueSql("getdate()");   

            modelBuilder.Entity<Item>()
                .Property(i => i.CreatedAt)
                .HasDefaultValueSql("getdate()");

            modelBuilder.Entity<Item>()
                .Property(i => i.UpdateAt)
                .HasDefaultValueSql("getdate()"); 
        }
    }
}