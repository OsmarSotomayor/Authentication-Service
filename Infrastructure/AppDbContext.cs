using Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace Infrastructure
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
      : base(options) { }

        public DbSet<User> Users => Set<User>();
        public DbSet<LoginAttempt> LoginAttempts => Set<LoginAttempt>();

        public DbSet<BlackListToken> BlacklistedTokens => Set<BlackListToken>();
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Username).IsRequired().HasMaxLength(100);
                entity.Property(e => e.PasswordHash).IsRequired();
                entity.HasMany(e => e.LoginAttempts)
                      .WithOne(e => e.User!)
                      .HasForeignKey(e => e.UserId);
            });

            modelBuilder.Entity<LoginAttempt>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.IsSuccess).IsRequired();
                entity.Property(e => e.AttemptedAt).IsRequired();
            });

            modelBuilder.Entity<BlackListToken>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Token).IsRequired();
                entity.Property(e => e.ExpirationDate).IsRequired();
            });
        }
    }

    public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
    {
        public AppDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
            optionsBuilder.UseSqlServer("Server=tcp:servidoromartest.database.windows.net,1433;Initial Catalog=Test;Persist Security Info=False;User ID=inicio_sql;Password=Soto17b*;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;");

            return new AppDbContext(optionsBuilder.Options);
        }
    }
}
