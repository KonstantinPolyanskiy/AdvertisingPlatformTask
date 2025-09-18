using Core.Application.Models;
using Core.Data.Storages;
using FluentAssertions;

namespace Core.Data.Tests.AdPlatformStorageTests;

public class InMemoryAdPlatformStorageTests
{
    #region FindAsync

    /// <summary>
    /// Тест поиска на основе данных, приведенных в тестовом задании.
    /// </summary>
    [Fact]
    public async Task FindAsync_FindsRecord_For_Ancestors_Data()
    {
        // Arrange
        var storage = new InMemoryAdPlatformStorage();
        await storage.ReplaceAsync(BuildSampleRecords());

        // Act
        var resultRu = await storage.FindAsync("/ru");
        var resultSvrd = await storage.FindAsync("/ru/svrd");
        var resultRevda = await storage.FindAsync("/ru/svrd/revda");
        var resultMsk = await storage.FindAsync("/ru/msk");
        
        // Assert
        resultRu.Should().BeEquivalentTo("Яндекс.Директ");
        resultSvrd.Should().BeEquivalentTo("Яндекс.Директ", "Крутая реклама");
        resultRevda.Should().BeEquivalentTo("Яндекс.Директ", "Крутая реклама", "Ревдинский рабочий");
        resultMsk.Should().BeEquivalentTo("Яндекс.Директ", "Газета уральских москвичей");
    }

    /// <summary>
    /// Тест на проверку условия, что "чем меньше вложенность локации, тем глобальнее действует рекламная площадка".
    /// </summary>
    [Fact]
    public async Task FindAsync_FindNearestRecord_For_Unknown_Location()
    {
        // Arrange
        var storage = new InMemoryAdPlatformStorage();
        await storage.ReplaceAsync(BuildSampleRecords());

        // Act
        var result = await storage.FindAsync("/ru/svrd/unknown");

        // Assert
        result.Should().BeEquivalentTo("Яндекс.Директ", "Крутая реклама");
    }

    #endregion

    #region ReplaceAsync

    /// <summary>
    /// Тест на то, что старые данные при загрузке новых удаляются.
    /// </summary>
    [Fact]
    public async Task ReplaceAsync_CheckOverwriteData()
    {
        // Arrange
        var storage = new InMemoryAdPlatformStorage();
        await storage.ReplaceAsync(BuildSampleRecords());

        var newRecords = new List<AdRecord>
        {
            R("Новая площадка", "/ru/svrd")
        };

        // Act
        await storage.ReplaceAsync(newRecords);
        var after = await storage.FindAsync("/ru/svrd");

        // Assert
        after.Should().BeEquivalentTo("Новая площадка");
        after.Should().NotContain(["Крутая реклама", "Яндекс.Директ"]);
    }

    #endregion

    #region Helpers

    private static AdRecord R(string platform, params string[] locations)
    {
        if (!AdRecord.TryCreate(platform, locations, out var rec, out var error))
        {
            throw new InvalidOperationException($"Невалидные данные для теста - '{platform}': {error}");
        }

        return rec!;
    }

    private static IList<AdRecord> BuildSampleRecords() => new List<AdRecord>
    {
        R("Яндекс.Директ", "/ru"),
        R("Газета уральских москвичей", "/ru/msk", "/ru/permobl", "/ru/chelobl"),
        R("Крутая реклама", "/ru/svrd"),
        R("Ревдинский рабочий", "/ru/svrd/revda", "/ru/svrd/pervik"),
    };

    #endregion
}