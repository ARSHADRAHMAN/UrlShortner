using Microsoft.EntityFrameworkCore;
using UrlShortner.Models;

namespace UrlShortner.Data;

/// <summary>
/// Entity Framework Core DbContext for URL Shortener
/// Handles database configuration, entity mappings, and relationships
/// </summary>
public class UrlShortenerDbContext : DbContext
{
    public UrlShortenerDbContext(DbContextOptions<UrlShortenerDbContext> options)
        : base(options)
    {
    }

    /// <summary>
    /// DbSet for URL shortener entries
    /// </summary>
    public required DbSet<UrlShortenerEntry> UrlEntries { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure UrlShortenerEntry entity
        var urlEntry = modelBuilder.Entity<UrlShortenerEntry>();

        // Primary key
        urlEntry.HasKey(x => x.Id);

        // Properties configuration with constraints and defaults
        urlEntry.Property(x => x.Id)
            .ValueGeneratedNever()
            .IsRequired();

        urlEntry.Property(x => x.OriginalUrl)
            .IsRequired()
            .HasMaxLength(2048);

        urlEntry.Property(x => x.ShortCode)
            .IsRequired()
            .HasMaxLength(50);

        urlEntry.Property(x => x.CustomAlias)
            .HasMaxLength(50);

        urlEntry.Property(x => x.Description)
            .HasMaxLength(500);

        urlEntry.Property(x => x.OwnerId)
            .HasMaxLength(128);

        urlEntry.Property(x => x.CreatedAt)
            .IsRequired()
            .HasDefaultValueSql("GETUTCDATE()");

        urlEntry.Property(x => x.UpdatedAt)
            .IsRequired()
            .HasDefaultValueSql("GETUTCDATE()");

        urlEntry.Property(x => x.ClickCount)
            .IsRequired()
            .HasDefaultValue(0);

        urlEntry.Property(x => x.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        // Column configuration for nullable DateTime
        urlEntry.Property(x => x.ExpiresAt)
            .HasColumnType("datetime2");

        urlEntry.Property(x => x.LastVisited)
            .HasColumnType("datetime2");

        // Unique Constraints
        urlEntry.HasIndex(x => x.ShortCode)
            .IsUnique()
            .HasDatabaseName("IX_ShortCode");

        urlEntry.HasIndex(x => x.CustomAlias)
            .IsUnique()
            .HasFilter("[CustomAlias] IS NOT NULL")
            .HasDatabaseName("IX_CustomAlias");

        // Performance Indexes for filtering and sorting
        urlEntry.HasIndex(x => x.CreatedAt)
            .HasDatabaseName("IX_CreatedAt")
            .IsDescending(false);

        urlEntry.HasIndex(x => x.ExpiresAt)
            .HasFilter("[ExpiresAt] IS NOT NULL")
            .HasDatabaseName("IX_ExpiresAt");

        urlEntry.HasIndex(x => x.IsActive)
            .HasDatabaseName("IX_IsActive");

        urlEntry.HasIndex(x => x.ClickCount)
            .HasDatabaseName("IX_ClickCount")
            .IsDescending(true); // Descending for "most popular" queries

        urlEntry.HasIndex(x => x.LastVisited)
            .HasFilter("[LastVisited] IS NOT NULL")
            .HasDatabaseName("IX_LastVisited");

        urlEntry.HasIndex(x => x.OwnerId)
            .HasFilter("[OwnerId] IS NOT NULL")
            .HasDatabaseName("IX_OwnerId");

        // Composite indexes for common query patterns
        urlEntry.HasIndex(x => new { x.OwnerId, x.IsActive, x.CreatedAt })
            .HasDatabaseName("IX_Owner_Active_Created")
            .IsDescending(false, false, true); // Descending on CreatedAt for newest first

        urlEntry.HasIndex(x => new { x.IsActive, x.ExpiresAt })
            .HasFilter("[IsActive] = 1")
            .HasDatabaseName("IX_Active_Expiration")
            .IsDescending(false, false);

        // Configure table name
        urlEntry.ToTable("UrlEntries", t => t.HasComment("Shortened URL entries with analytics"));
    }
}
