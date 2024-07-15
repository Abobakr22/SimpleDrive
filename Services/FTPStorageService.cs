using FluentFTP;
using Microsoft.Extensions.Options;
using SimpleDrive.Models;
using SimpleDrive.Models.Settings;
using SimpleDrive.Services.IServices;

namespace SimpleDrive.Services
{
    public class FTPStorageService : IStorageService
    {
        private readonly FTPStorageSettings _settings;

        public FTPStorageService(IOptions<FTPStorageSettings> options)
        {
            _settings = options.Value;
        }

        public async Task StoreObjectAsync(string id, byte[] data)
        {
            await Task.Run(() =>
            {
                using(var client = new FtpClient(_settings.Host, _settings.Username, _settings.Password))
            {
                client.Connect();

                using(var stream = new MemoryStream(data))
                {
                    client.UploadStream(stream, id);
                }
            }
            });
        }

        public async Task<Blob> RetreiveObjectAsync(string id)
        {
            return await Task.Run(() =>
            {
                using(var client = new FtpClient(_settings.Host, _settings.Username, _settings.Password))
                {
                    client.Connect();

                    using(var stream = new MemoryStream())
                    {
                        client.DownloadStream(stream, id);
                        var data = stream.ToArray();

                        var blob = new Blob
                        {
                            Id = id,
                            Data = data,
                            Size = data.Length,
                            CreatedAt = DateTime.UtcNow
                        };

                        return Task.FromResult(blob);
                    }
                }
            });
        }

    }
}