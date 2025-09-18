using Core.Application.Models;

namespace Core.Application.Services.AdPlatformService.Contract;

public interface IAdPlatformService
{
    /// <summary>
    /// Загрузка данных из потока, нормализация их и сохраняет в хранилище.
    /// </summary>
    /// <param name="inputStream">Входной поток данных.</param>
    /// <param name="ct">Токен отмены.</param>
    /// <returns>Количество сохраненных строк.</returns>
    Task<UploadRecordsResult> UploadDataFromStreamAsync(Stream inputStream, CancellationToken ct = default);

    /// <summary>
    /// Поиск рекламной площадки по локации (в т.ч. и не нормализованной). 
    /// </summary>
    /// <param name="location">Локация, по которой происходит поиск.</param>
    /// <param name="ct">Токен отмены.</param>
    /// <returns>Площадки, которые содержат переданные локации.</returns>
    Task<IReadOnlyCollection<string>> FindAsync(string location, CancellationToken ct = default);
}