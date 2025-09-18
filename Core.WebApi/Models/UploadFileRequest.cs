using System.ComponentModel.DataAnnotations;

namespace Core.WebApi.Models;

public sealed class UploadFileRequest
{
    [Required]
    public IFormFile File { get; init; } = default!;
}