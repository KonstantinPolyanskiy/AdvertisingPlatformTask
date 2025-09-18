using Core.Application.Models;
using Core.Application.Services.AdPlatformService.Contract;
using Core.WebApi.Models;
using Microsoft.AspNetCore.Mvc;

namespace Core.WebApi.Controllers;

[ApiController]
[Route("api/ad-platform")]
[Produces("application/json")]
public sealed class AdPlatformController(IAdPlatformService service) : ControllerBase
{
    [HttpPost("upload")]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(typeof(UploadRecordsResult), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<UploadRecordsResult>> UploadFileAsync([FromForm] UploadFileRequest request,
        CancellationToken ct)
    {
        if (request.File.Length == 0)
        {
            return ValidationProblem();
        }

        await using var stream = request.File.OpenReadStream();

        var report = await service.UploadDataFromStreamAsync(stream, ct);

        return Ok(report);
    }

    [HttpGet("search")]
    [ProducesResponseType(typeof(IReadOnlyCollection<string>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyCollection<string>>> SearchAsync(
        [FromQuery] string location,
        CancellationToken ct)
    {
        var result = await service.FindAsync(location, ct);
        return Ok(result);
    }
}