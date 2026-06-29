using UrlShortner.Models;

namespace UrlShortner.Repositories;

/// <summary>
/// In-memory implementation of IUrlRepository
/// Provides CRUD operations for URL shortener entries
/// This can be replaced with a database implementation in the future
/// </summary>
public class UrlRepository : IUrlRepository
{
    /// <summary>
    /// In-memory collection to store URL entries
    /// In production, this would be replaced with a database
    /// </summary>
    private readonly List<UrlShortenerEntry> _urlEntries = [];

    /// <summary>
    /// Lock object for thread-safe operations in multi-threaded environment
    /// </summary>
    private readonly object _lockObject = new();

    public async Task<UrlShortenerEntry> CreateAsync(UrlShortenerEntry entry, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(entry);

        // Simulate async operation
        await Task.Delay(0, cancellationToken).ConfigureAwait(false);

        lock (_lockObject)
        {
            entry.Id = Guid.NewGuid();
            entry.CreatedAt = DateTime.UtcNow;
            entry.UpdatedAt = DateTime.UtcNow;
            _urlEntries.Add(entry);
        }

        return entry;
    }

    public async Task<UrlShortenerEntry?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        // Simulate async operation
        await Task.Delay(0, cancellationToken).ConfigureAwait(false);

        lock (_lockObject)
        {
            return _urlEntries.FirstOrDefault(x => x.Id == id);
        }
    }

    public async Task<UrlShortenerEntry?> GetByShortCodeAsync(string shortCode, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(shortCode);

        // Simulate async operation
        await Task.Delay(0, cancellationToken).ConfigureAwait(false);

        lock (_lockObject)
        {
            return _urlEntries.FirstOrDefault(x => x.ShortCode == shortCode);
        }
    }

    public async Task<UrlShortenerEntry?> GetByOriginalUrlAsync(string originalUrl, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(originalUrl);
        await Task.Delay(0, cancellationToken).ConfigureAwait(false);

        lock (_lockObject)
        {
            return _urlEntries.FirstOrDefault(x => x.OriginalUrl == originalUrl);
        }
    }



    public async Task<(List<UrlShortenerEntry> Items, int TotalCount)> GetAllAsync(int pageNumber = 1, int pageSize = 10, CancellationToken cancellationToken = default)
    {
        // Simulate async operation
        await Task.Delay(0, cancellationToken).ConfigureAwait(false);

        lock (_lockObject)
        {
            var totalCount = _urlEntries.Count;
            var skip = (pageNumber - 1) * pageSize;
            var items = _urlEntries
                .Skip(skip)
                .Take(pageSize)
                .ToList();

            return (items, totalCount);
        }
    }

    public async Task<UrlShortenerEntry> UpdateAsync(UrlShortenerEntry entry, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(entry);

        // Simulate async operation
        await Task.Delay(0, cancellationToken).ConfigureAwait(false);

        lock (_lockObject)
        {
            var existingEntry = _urlEntries.FirstOrDefault(x => x.Id == entry.Id);
            if (existingEntry == null)
            {
                throw new KeyNotFoundException($"URL entry with ID {entry.Id} not found");
            }

            existingEntry.OriginalUrl = entry.OriginalUrl;
            existingEntry.ClickCount = entry.ClickCount;
            existingEntry.LastVisited = entry.LastVisited;
            existingEntry.UpdatedAt = DateTime.UtcNow;

            return existingEntry;
        }
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        // Simulate async operation
        await Task.Delay(0, cancellationToken).ConfigureAwait(false);

        lock (_lockObject)
        {
            var entry = _urlEntries.FirstOrDefault(x => x.Id == id);
            if (entry == null)
            {
                return false;
            }

            return _urlEntries.Remove(entry);
        }
    }

    public async Task<bool> ShortCodeExistsAsync(string shortCode, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(shortCode);

        // Simulate async operation
        await Task.Delay(0, cancellationToken).ConfigureAwait(false);

        lock (_lockObject)
        {
            return _urlEntries.Any(x => x.ShortCode == shortCode);
        }
    }

    public async Task<bool> IncrementAccessCountAsync(Guid id, CancellationToken cancellationToken = default)
    {
        // Simulate async operation
        await Task.Delay(0, cancellationToken).ConfigureAwait(false);

        lock (_lockObject)
        {
            var entry = _urlEntries.FirstOrDefault(x => x.Id == id);
            if (entry == null)
            {
                return false;
            }

            entry.RecordClick();
            return true;
        }
    }
}
