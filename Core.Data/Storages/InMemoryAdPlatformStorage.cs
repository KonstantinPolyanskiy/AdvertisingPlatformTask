using System.Collections.Immutable;
using System.Text.RegularExpressions;
using Core.Application.Contracts;
using Core.Application.Models;

namespace Core.Data.Storages;

public sealed class InMemoryAdPlatformStorage : IAdPlatformReader, IAdPlatformWriter
{
    /// <summary>
    /// Семафор для предотвращения параллельных <see cref="ReplaceAsync"/>, т.к. это долгая операция.
    /// </summary>
    private readonly SemaphoreSlim _semaphore = new(1, 1);

    private ImmutableDictionary<string, ImmutableHashSet<string>> _cumulative =
        ImmutableDictionary<string, ImmutableHashSet<string>>.Empty.WithComparers(StringComparer.Ordinal);
        
   /// <inheritdoc />
    public Task<IReadOnlyCollection<string>> FindAsync(string location, CancellationToken ct = default)
    {
        if (_cumulative.TryGetValue(location, out var set))
            return Task.FromResult<IReadOnlyCollection<string>>(set);

        var current = location;
        while (true)
        {
            var slash = current.LastIndexOf('/');
            if (slash <= 0)
            {
                break;
            }

            current = current[..slash];

            if (_cumulative.TryGetValue(current, out set))
            {
                return Task.FromResult<IReadOnlyCollection<string>>(set);
            }
        }

        if (_cumulative.TryGetValue("/", out set))
            return Task.FromResult<IReadOnlyCollection<string>>(set);

        return Task.FromResult<IReadOnlyCollection<string>>(Array.Empty<string>());
    }

    /// <inheritdoc />
    public async Task ReplaceAsync(IList<AdRecord> records, CancellationToken ct = default)
    {
        await _semaphore.WaitAsync(ct).ConfigureAwait(false);
        try
        {
            // Собираем словарь кореневых локаций
            var parent = BuildDeclaredDictionary(records, ct);

            // Строим граф от родителя (корня) к детям 
            var children = BuildChildrenDictionary(parent.Keys);

            // Строим индекс на основе множеств путей, определенных в родительском и дочерних словарях
            var accumulated = BuildCumulativeIndex(parent, children, ct);

            Interlocked.Exchange(ref _cumulative, accumulated);
        }
        finally
        {
            _semaphore.Release();
        }
    }

    private static Dictionary<string, HashSet<string>> BuildDeclaredDictionary(IList<AdRecord> records, CancellationToken ct)
    {
        var declared = new Dictionary<string, HashSet<string>>(StringComparer.Ordinal);

        foreach (var record in records)
        {
            ct.ThrowIfCancellationRequested();
            
            var platform = record.Platform?.Trim();

            if (string.IsNullOrEmpty(platform) || record.Locations.Count == 0)
            {
                continue;
            }

            foreach (var location in record.Locations)
            {
                // Добавляем рекламную площадку для конкретной локации
                if (!declared.TryGetValue(location, out var set))
                {
                    set = new HashSet<string>(StringComparer.Ordinal);
                    declared[location] = set;
                }

                set.Add(platform);

                EnsureAllAncestorsDeclared(location, declared);
            }
        }
        
        // Гарантируем наличие корня 
        declared.TryAdd("/", new HashSet<string>(StringComparer.Ordinal));

        return declared;
    }

    private static void EnsureAllAncestorsDeclared(string path, Dictionary<string, HashSet<string>> declared)
    {
        string parent = string.Empty;

        while (true)
        {
            var slash = path.LastIndexOf('/');
            if (slash <= 0)
            {
                break;
            }
            
            parent = parent[..slash];

            declared.TryAdd(parent, new HashSet<string>(StringComparer.Ordinal));
        }

        declared.TryAdd("/", new HashSet<string>(StringComparer.Ordinal));
    }

    private static Dictionary<string, List<string>> BuildChildrenDictionary(IEnumerable<string> allPaths)
    {
        var children = new Dictionary<string, List<string>>(StringComparer.Ordinal);

        foreach (var path in allPaths)
        {
            var parent = GetParent(path);
            if (!children.TryGetValue(parent, out var list))
            {
                list = new List<string>();

                children[parent] = list;
            }

            if (path != parent)
            {
                list.Add(path);
            }
        }

        children.TryAdd("/", new List<string>());
        
        return children;
    }

    private static ImmutableDictionary<string, ImmutableHashSet<string>> BuildCumulativeIndex(
        Dictionary<string, HashSet<string>> declared,
        Dictionary<string, List<string>> children,
        CancellationToken ct)
    {
        var builder = ImmutableDictionary.CreateBuilder<string, ImmutableHashSet<string>>(StringComparer.Ordinal);
        var queue = new Queue<string>();

        builder["/"] = declared["/"].ToImmutableHashSet(StringComparer.Ordinal);
        
        queue.Enqueue("/");

        while (queue.Count > 0)
        {
            ct.ThrowIfCancellationRequested();

            var parent = queue.Dequeue();
            if (!children.TryGetValue(parent, out var kids))
            {
                continue;
            }

            var parentCumulative = builder[parent]; 

            foreach (var child in kids)
            {
                var hb = parentCumulative.ToBuilder();  
                
                hb.UnionWith(declared[child]);      

                builder[child] = hb.ToImmutable();  

                queue.Enqueue(child);
            }
        }

        return builder.ToImmutable();
    }

    private static string GetParent(string path)
    {
        if (path == "/") return "/";
        var slash = path.LastIndexOf('/');
        return slash <= 0 ? "/" : path[..slash];
    }
}