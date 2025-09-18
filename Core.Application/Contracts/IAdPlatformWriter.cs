using Core.Application.Models;

namespace Core.Application.Contracts;

/// <summary>
/// Запись данных рекламных площадок в хранилище.
/// </summary>
public interface IAdPlatformWriter
{                 
    /// <summary>
    /// Очищает предыдущие записи из хранилища и вставляет в него переданные.
    /// </summary>
    /// <param name="records">Записи.</param>
    /// <param name="ct">Токен отмены.</param>
    Task ReplaceAsync(IList<AdRecord> records, CancellationToken ct = default);
}