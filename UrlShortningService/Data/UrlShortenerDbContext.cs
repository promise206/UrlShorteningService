using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Reflection.Emit;
using System.Xml.Serialization;
using UrlShortningService.Domain.Models;

namespace UrlShortningService.Data
{
    public class UrlShortenerDbContext : DbContext
    {
        public DbSet<UrlMap> UrlMappings { get; set; }

        public UrlShortenerDbContext(DbContextOptions<UrlShortenerDbContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<UrlMap>()
                .HasIndex(u => u.ShortUrl)
                .IsUnique(); // Ensure short URLs are unique

            base.OnModelCreating(modelBuilder);
        }
    }
}
