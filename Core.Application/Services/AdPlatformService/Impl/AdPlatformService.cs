using System.Text;
using Core.Application.Contracts;
using Core.Application.Models;
using Core.Application.Normalizers;
using Core.Application.Services.AdPlatformService.Contract;

namespace Core.Application.Services.AdPlatformService.Impl;

/// <summary>
/// Сервис для работы с рекламными площадками.
/// </summary>
public sealed class AdPlatformService(IAdPlatformReader reader, IAdPlatformWriter writer) : IAdPlatformService
{
    /// <inheritdoc />
    public async Task<UploadRecordsResult> UploadDataFromStreamAsync(Stream inputStream, CancellationToken ct = default)
    {
        var result = new UploadRecordsResult();

        var records = new List<AdRecord>();

        using var reader = new StreamReader(inputStream, Encoding.UTF8, detectEncodingFromByteOrderMarks: true);

        string? line;

        while ((line = await reader.ReadLineAsync(ct).ConfigureAwait(false)) is not null)
        {
            ct.ThrowIfCancellationRequested();

            line = line.Trim();

            // Разбиваем площадку и локации
            var colonIndex = line.IndexOf(':');

            if (colonIndex <= 0 || colonIndex >= line.Length - 1)
            {
                result.Errors.Add($"Строка {line}: отсутствует ':' или список локаций.");
                continue;
            }

            var platform = line[..colonIndex].Trim();
            var rawLocations = line[(colonIndex + 1)..]
                .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

            var normalizedLocations = new List<string>(rawLocations.Length);

            foreach (var rawLocation in rawLocations)
            {
                result.Total += 1;

                if (DefaultLocationNormalizer.TryNormalize(rawLocation, out var location))
                {
                    normalizedLocations.Add(location);
                }
                else
                {
                    result.Errors.Add($"Строка {line}: локация '{rawLocation}' некорректна.");
                }
            }

            if (!AdRecord.TryCreate(platform, normalizedLocations, out var record, out var err))
            {
                result.Errors.Add($"Возникла ошибка {err}");
                continue;
            }

            records.Add(record!);

            result.Accepted++;
        }

        await writer.ReplaceAsync(records, ct);

        return result;
    }

    /// <inheritdoc />
    public Task<IReadOnlyCollection<string>> FindAsync(string location, CancellationToken ct = default)
    {
        if (!DefaultLocationNormalizer.TryNormalize(location, out var normalized))
        {
            return Task.FromResult<IReadOnlyCollection<string>>(Array.Empty<string>());
        }

        return reader.FindAsync(normalized, ct);
    }
}