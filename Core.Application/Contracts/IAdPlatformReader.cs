namespace Core.Application.Contracts;

/// <summary>
/// Чтение данных рекламных площадок из хранилища.
/// </summary>
public interface IAdPlatformReader
{
    /// <summary>
    /// Поиск рекламных площадок по переданной локации.
    /// </summary>
    /// <param name="location">Локация ("/", "/ru", "/ru/msk").</param>
    /// <param name="ct">Токен отмены.</param>
    /// <returns>Список площадок, подходящий для локации и всех ее префиксах.</returns>
    Task<IReadOnlyCollection<string>> FindAsync(string location, CancellationToken ct = default);
}
