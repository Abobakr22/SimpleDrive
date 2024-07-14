using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SimpleDrive.Dtos;
using SimpleDrive.Services.IServices;

namespace SimpleDrive.Controllers
{
    [ApiController]
    public class BlobController : Controller
    {
        private readonly IStorageService _storageService;

        public BlobController(IStorageService storageService)
        {
            _storageService = storageService;
        }

        [HttpPost]
        [Route("v1/blobs")]
        [Authorize]
        public async Task<IActionResult> StoreBlob([FromBody] BlobRequestDto request)
        {
            if(string.IsNullOrEmpty(request.Id) || request.Data == null || request.Data.Length == 0)
            {
                return BadRequest("Invalid blob or object data");
            }

            try
            {
                var data = Convert.FromBase64String(Convert.ToBase64String(request.Data));
                await _storageService.StoreObjectAsync(request.Id, data);
                return Ok(data);
            }
            catch(Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }



        [HttpGet]
        [Route("v1/blobs/{id}")]
        [Authorize]
        public async Task<IActionResult> RetreiveBlob(string id)
        {
            var blob = await _storageService.RetreiveObjectAsync(id);
            if(blob == null)
            {
                return NotFound();
            }

            var response = new BlobResponseDto
            {
                Id = blob.Id,
                Data = Convert.ToBase64String(blob.Data),
                Size = blob.Size,
                CreatedAt = blob.CreatedAt,
            };

            return Ok(response);
        }
    }
}
