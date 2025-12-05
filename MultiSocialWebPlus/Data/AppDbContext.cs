using Microsoft.EntityFrameworkCore;
using System;
using System.IO;
using MultiSocialWebPlus.Models;

namespace MultiSocialWebPlus.Data
{
    public class AppDbContext : DbContext
    {
        public DbSet<Customer> Customers { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<Quote> Quotes { get; set; }
        public DbSet<QuoteItem> QuoteItems { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            var dataDir = Environment.GetEnvironmentVariable("MSWPLUS_DATA_DIR")
                       ?? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "MultiSocialWebPlus");
            Directory.CreateDirectory(dataDir);
            var dbPath = Path.Combine(dataDir, "multisocialplus.db");
            optionsBuilder.UseSqlite($"Data Source={dbPath}");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Customer>().ToTable("Customers");
            modelBuilder.Entity<Category>().ToTable("Categories");
            modelBuilder.Entity<Product>().ToTable("Products");
            modelBuilder.Entity<Quote>().ToTable("Quotes");
            modelBuilder.Entity<QuoteItem>().ToTable("QuoteItems");

            modelBuilder.Entity<Product>()
                .Property(p => p.Unit)
                .HasConversion<string>();
            base.OnModelCreating(modelBuilder);
        }
    }
}
