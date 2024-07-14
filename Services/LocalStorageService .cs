using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Extensions.Options;
using SimpleDrive.Models;
using SimpleDrive.Models.Settings;
using SimpleDrive.Services.IServices;

namespace SimpleDrive.Services
{
    public class LocalStorageService : IStorageService
    {
        private readonly string _storagePath;
        public LocalStorageService(IOptions<LocalStorageSettings> options)
        {
            _storagePath = options.Value.Path;
        }

        public async Task StoreObjectAsync(string id, byte[] data)
        {
            var filePath = Path.Combine(_storagePath, id);
            await File.WriteAllBytesAsync(filePath, data);
        }

        public async Task<Blob> RetreiveObjectAsync(string id)
        {
            var filePath = Path.Combine(_storagePath, id);
            if(!File.Exists(filePath))
            {
                return null;
            }
            var data = await File.ReadAllBytesAsync(filePath);
            var fileInfo = new FileInfo(filePath);

            return new Blob
            {
                Id = id,
                Data = data,
                Size = fileInfo.Length,
                CreatedAt = fileInfo.CreationTimeUtc
            };

        }
    }
}
