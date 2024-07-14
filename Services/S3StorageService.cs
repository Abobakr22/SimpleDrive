using Microsoft.Extensions.Options;
using SimpleDrive.Models;
using SimpleDrive.Models.Settings;
using SimpleDrive.Services.IServices;

namespace SimpleDrive.Services
{
    public class S3StorageService : IStorageService
    {
        //not allowed to use s3 libraries
        //Your challenge is to explore the S3 protocol and only use an HTTP client to store
        //and retrieve files from the service.

        //You are not allowed to use any S3 library, and in doing so your project will be
        //rejected.

        private readonly S3StorageSettings _storageS3;
        private readonly HttpClient _httpClient;
        public S3StorageService(IOptions<S3StorageSettings> options, HttpClient httpClient)
        {
            _storageS3 = options.Value;
            _httpClient = httpClient;
        }

        public async Task StoreObjectAsync(string id, byte[] data)
        {
            var url = $"{_storageS3.EndpointUrl}/{_storageS3.BucketName}/{id}";
            var request = new HttpRequestMessage(HttpMethod.Put, url)
            {
                Content = new ByteArrayContent(data)
            };

            request.Headers.Add("x-amz-content-sha256", Convert.ToBase64String(data));
            request.Headers.Add("Authorization", GenerateS3AuthorizationHeader(request));

            var response = await _httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();
        }

        public async Task<Blob> RetreiveObjectAsync(string id)
        {
            var url = $"{_storageS3.EndpointUrl} / {_storageS3.BucketName}/{id}";

            var request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.Add("Authorization", GenerateS3AuthorizationHeader(request));

            var response = await _httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();

            var data = await response.Content.ReadAsByteArrayAsync();
            var contentLength = response.Content.Headers.ContentLength ?? 0;

            return new Blob
            {
                Id = id,
                Data = data,
                Size = contentLength,
                CreatedAt = DateTime.UtcNow
            };
        }

        private string GenerateS3AuthorizationHeader(HttpRequestMessage request)
        {
            //Implement the GenerateS3AuthorizationHeader method in S3StorageService to generate the AWS Signature
            //Version 4 authorization header.This involves signing the request using HMAC-SHA256 with your AWS
            //credentials and appending the resulting signature to the request headers.

            throw new NotImplementedException();
        }
    }
}
