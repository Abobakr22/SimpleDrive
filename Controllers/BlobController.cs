using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SimpleDrive.Dtos;
using SimpleDrive.Models;
using SimpleDrive.Services.IServices;

namespace SimpleDrive.Controllers
{
    [ApiController]
    public class BlobController : Controller
    {
        private readonly IStorageService _storageService;
        private readonly AppDbContext _dbContext;

        public BlobController(IStorageService storageService, AppDbContext dbContext)
        {
            _storageService = storageService;
            _dbContext = dbContext;
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

                // Check if the blob already exists in the StorageSettings table
                var existingEntry = await _dbContext.StorageSettings.FirstOrDefaultAsync(s => s.BlobId == request.Id);

                if(existingEntry == null)
                {
                    // Log to StorageSettings table if it doesn't exist
                    var createdAt = DateTime.UtcNow;
                    await LogStorageOperation(request.Id, _storageService.GetType().Name, createdAt);
                }
                else
                {
                    return Conflict("Blob with the given ID already exists.");
                }

                return Ok($"Blob '{request.Id}' stored successfully.");
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


        private async Task LogStorageOperation(string blobId , string storageType , DateTime createdAt)
        {
            // Create a new StorageSettings object to track the operation
            var storageSettings = new StorageSettings
            {
                Id = Guid.NewGuid(),
                BlobId = blobId,
                StorageType = storageType,
                CreatedAt = createdAt,
            };

            _dbContext.StorageSettings.Add(storageSettings);
            await _dbContext.SaveChangesAsync();
        }
    }
}
