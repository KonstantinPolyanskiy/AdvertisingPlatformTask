namespace Core.Application.Models;

/// <summary>
/// Рекламная площадка и локации, в которых она действует.
/// </summary>
public sealed record AdRecord
{
    /// <summary>
    /// Рекламная площадка ("Яндекс.Директ", "Крутая Реклама").
    /// </summary>
    public string Platform { get; private set; }

    /// <summary>
    /// Локации, в которых действует <see cref="Platform"/>.
    /// </summary>
    public IReadOnlyCollection<string> Locations { get; private set; }

    private AdRecord(string platform, IReadOnlyCollection<string> locations)
    {
        Platform = platform;
        Locations = locations;
    }

    /// <summary>
    /// Попытаться создать запись о площадке и ее локациях.
    /// </summary>
    /// <param name="platform">Имя площадки.</param>
    /// <param name="locations">Список локаций.</param>
    public static bool TryCreate(
        string platform,
        IList<string> locations,
        out AdRecord? record,
        out string? error)
    {          
        record = null;
        error = null;

        if (string.IsNullOrWhiteSpace(platform))
        {
            error = "Платформа не должна быть пустой.";
            return false;
        }

        if (!locations.Any())
        {
            error = "У платформы должны быть локации.";
            return false;
        }

        record = new AdRecord(platform.Trim(), locations.ToArray());

        return true;
    }
}
