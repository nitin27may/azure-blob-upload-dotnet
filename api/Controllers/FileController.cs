using AzureBlobApi.Extensions;
using AzureBlobApi.Services;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Headers;

namespace AzureBlobApi.Controllers;

[Route("api/[controller]")]
[ApiController]
public class FileController : ControllerBase
{
    private readonly IBlobService _blobService;

    private readonly ILogger _logger;

    public FileController(IBlobService blobService, ILogger<FileController> logger)
    {
        _blobService = blobService;
        _logger = logger;
    }

    [HttpPost, DisableRequestSizeLimit]
    public async Task<IActionResult> UploadAsync()
    {
        var formCollection = await Request.ReadFormAsync();
        var file = formCollection.Files.First();
        if (file.Length > 0)
        {
            string fileName = ContentDispositionHeaderValue.Parse(file.ContentDisposition).FileName.Trim('"');
            string fileURL = await _blobService.UploadAsync(file.OpenReadStream(), "videos", fileName.AppendTimeStamp(), file.ContentType);
            return Ok(new { fileURL });
        }

        return BadRequest();
    }

    [HttpGet("{containerName}")]
    public async Task<IActionResult> GetFiles(string containerName)
    {
        return Ok(await _blobService.GetFiles(containerName, null));
    }
}