using SimpleDrive.Models;

namespace SimpleDrive.Services.IServices
{
    public interface IStorageService
    {
        Task StoreObjectAsync(string id, byte[] data);
        Task<Blob> RetreiveObjectAsync(string id);
    }
}
