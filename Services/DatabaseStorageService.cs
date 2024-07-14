using Microsoft.Extensions.Options;
using SimpleDrive.Models;
using SimpleDrive.Models.Settings;
using SimpleDrive.Services.IServices;

namespace SimpleDrive.Services
{
    public class DatabaseStorageService : IStorageService
    {
        private readonly DatabaseStorageSettings _settings;
        private readonly AppDbContext _context;

        public DatabaseStorageService(IOptions<DatabaseStorageSettings> options, AppDbContext appDbContext)
        {
            _settings = options.Value;
            _context = appDbContext;
        }

        public async Task StoreObjectAsync(string id, byte[] data)
        {
            var blob = new Blob
            {
                Id = id,
                Data = data,
                Size = data.Length,
                CreatedAt = DateTime.UtcNow
            };

            _context.Blobs.Add(blob);
            await _context.SaveChangesAsync();
        }


        public async Task<Blob> RetreiveObjectAsync(string id)
        {
            return await _context.Blobs.FindAsync(id);
        }
    }
}
