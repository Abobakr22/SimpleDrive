using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using SimpleDrive.Models;
using System.Reflection.Metadata;

public class AppDbContext : IdentityDbContext<ApplicationUser>
{
    public DbSet<SimpleDrive.Models.Blob> Blobs { get; set; }

    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.Entity<SimpleDrive.Models.Blob>().HasKey(b => b.Id);
    }
}