using Microsoft.EntityFrameworkCore;
using UrlShortner.Data;
using UrlShortner.Models;

namespace UrlShortner.Repositories;

/// <summary>
/// Entity Framework Core implementation of IUrlRepository
/// Provides database persistence using SQL Server or SQLite
/// Implements all repository operations asynchronously
/// </summary>
public class EfCoreUrlRepository : IUrlRepository
{
    private readonly UrlShortenerDbContext _dbContext;

    public EfCoreUrlRepository(UrlShortenerDbContext dbContext)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
    }

    public async Task<UrlShortenerEntry> CreateAsync(UrlShortenerEntry entry, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(entry);

        if (entry.Id == Guid.Empty)
        {
            entry.Id = Guid.NewGuid();
        }

        entry.CreatedAt = DateTime.UtcNow;
        entry.UpdatedAt = DateTime.UtcNow;

        _dbContext.UrlEntries.Add(entry);
        await _dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return entry;
    }

    public async Task<UrlShortenerEntry?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbContext.UrlEntries
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken)
            .ConfigureAwait(false);
    }

    public async Task<UrlShortenerEntry?> GetByShortCodeAsync(string shortCode, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(shortCode);

        return await _dbContext.UrlEntries
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.ShortCode == shortCode, cancellationToken)
            .ConfigureAwait(false);
    }

    public async Task<UrlShortenerEntry?> GetByOriginalUrlAsync(string originalUrl, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(originalUrl);

        return await _dbContext.UrlEntries
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.OriginalUrl == originalUrl, cancellationToken)
            .ConfigureAwait(false);
    }



    public async Task<(List<UrlShortenerEntry> Items, int TotalCount)> GetAllAsync(int pageNumber = 1, int pageSize = 10, CancellationToken cancellationToken = default)
    {
        if (pageNumber < 1)
            throw new ArgumentException("Page number must be greater than 0", nameof(pageNumber));

        if (pageSize < 1 || pageSize > 100)
            throw new ArgumentException("Page size must be between 1 and 100", nameof(pageSize));

        var query = _dbContext.UrlEntries.AsNoTracking();

        var totalCount = await query.CountAsync(cancellationToken).ConfigureAwait(false);

        var skip = (pageNumber - 1) * pageSize;
        var items = await query
            .OrderByDescending(x => x.CreatedAt)
            .Skip(skip)
            .Take(pageSize)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        return (items, totalCount);
    }

    public async Task<UrlShortenerEntry> UpdateAsync(UrlShortenerEntry entry, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(entry);

        var existingEntry = await _dbContext.UrlEntries
            .FirstOrDefaultAsync(x => x.Id == entry.Id, cancellationToken)
            .ConfigureAwait(false);

        if (existingEntry == null)
        {
            throw new KeyNotFoundException($"URL entry with ID {entry.Id} not found");
        }

        // Update only allowed fields
        existingEntry.OriginalUrl = entry.OriginalUrl;
        existingEntry.ClickCount = entry.ClickCount;
        existingEntry.LastVisited = entry.LastVisited;
        existingEntry.UpdatedAt = DateTime.UtcNow;

        _dbContext.UrlEntries.Update(existingEntry);
        await _dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return existingEntry;
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entry = await _dbContext.UrlEntries
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken)
            .ConfigureAwait(false);

        if (entry == null)
        {
            return false;
        }

        _dbContext.UrlEntries.Remove(entry);
        await _dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return true;
    }

    public async Task<bool> ShortCodeExistsAsync(string shortCode, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(shortCode);

        return await _dbContext.UrlEntries
            .AsNoTracking()
            .AnyAsync(x => x.ShortCode == shortCode, cancellationToken)
            .ConfigureAwait(false);
    }

    public async Task<bool> IncrementAccessCountAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entry = await _dbContext.UrlEntries
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken)
            .ConfigureAwait(false);

        if (entry == null)
        {
            return false;
        }

        entry.RecordClick();

        _dbContext.UrlEntries.Update(entry);
        await _dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return true;
    }
}
