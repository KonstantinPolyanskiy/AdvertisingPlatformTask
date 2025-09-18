using Core.Application.Services.AdPlatformService.Contract;

namespace Core.Application.Models;

public sealed class UploadRecordsResult
{
    public int Total { get; set; }

    public int Accepted { get; set; }

    public IList<string> Errors { get; set; } = new List<string>();
}