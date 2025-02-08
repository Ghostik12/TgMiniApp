using Microsoft.EntityFrameworkCore;
using LanguageBot.Models;

namespace LanguageBot.DB
{
    public class AppDbContext : DbContext
    {
        public DbSet<User> Users { get; set; }
        public DbSet<Word> Words { get; set; }
        public DbSet<Exercise> Exercises { get; set; }
        public DbSet<Achievement> Achievements { get; set; }
        public DbSet<Lesson> Lessons { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            // Подключение к PostgreSQL
            optionsBuilder.UseNpgsql("Host=localhost;Database=lingua_bot1;Username=postgres;Password=12345Ob@");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Achievement>()
                .HasOne(a => a.User)
                .WithMany(u => u.Achievements)
                .HasForeignKey(a => a.UserId)
                .HasPrincipalKey(u => u.ChatId);
        }
    }
}
