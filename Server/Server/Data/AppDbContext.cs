using Microsoft.EntityFrameworkCore;
using Server.Models;
using System.Collections.Generic;

namespace Server.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        public DbSet<Canvas> Canvases { get; set; }
        public DbSet<DrawingInCanvas> DrawingsInCanvas { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<DrawingInCanvas>()
                .HasOne(d => d.Canvas)
                .WithMany(c => c.Drawings)
                .HasForeignKey(d => d.CanvasId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
