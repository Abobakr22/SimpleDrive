using Microsoft.Extensions.Options;
using SimpleDrive.Models;
using SimpleDrive.Models.Settings;
using SimpleDrive.Services.IServices;
using System.Net;
using System.Security.Cryptography;
using System.Security.Cryptography.Xml;
using System.Text;

namespace SimpleDrive.Services
{
    public class S3StorageService : IStorageService
    {
        //not allowed to use s3 libraries

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
            var url = $"{_storageS3.EndpointUrl}/{_storageS3.BucketName}/{id}";
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
            // Extract information from app settings
            string accessKey = _storageS3.AccessKey;
            string secretKey = _storageS3.SecretKey;
            string service = "s3";
            string region = "eu-north-1";
            string endpoint = _storageS3.EndpointUrl;

            // Prepare current date and timestamp
            DateTime now = DateTime.UtcNow;
            string amzDate = now.ToString("yyyyMMddTHHmmssZ");
            string dateStamp = now.ToString("yyyyMMdd");

            // Extract URI components for canonical request
            string canonicalUri = request.RequestUri.AbsolutePath;
            string canonicalQueryString = request.RequestUri.Query;

            // Create canonical headers and signed headers
            string canonicalHeaders = $"host:{endpoint.Split('/')[2]}\n" +
                                      $"x-amz-content-sha256:UNSIGNED-PAYLOAD\n" +
                                      $"x-amz-date:{amzDate}\n";
            string signedHeaders = "host;x-amz-content-sha256;x-amz-date";

            // Hash of the payload (UNSIGNED-PAYLOAD in case of PUT requests)
            string payloadHash = "UNSIGNED-PAYLOAD";

            // Create canonical request
            string canonicalRequest = $"{request.Method}\n" +
                                      $"{canonicalUri}\n" +
                                      $"{canonicalQueryString}\n" +
                                      $"{canonicalHeaders}\n" +
                                      $"{signedHeaders}\n" +
                                      $"{payloadHash}";

            // Create string to sign
            string algorithm = "AWS4-HMAC-SHA256";
            string credentialScope = $"{dateStamp}/{region}/{service}/aws4_request";
            string stringToSign = $"{algorithm}\n" +
                                  $"{amzDate}\n" +
                                  $"{credentialScope}\n" +
                                  $"{Hash(canonicalRequest)}";

            // Generate signing key
            byte[] signingKey = GetSignatureKey(secretKey, dateStamp, region, service);

            // Calculate signature
            byte[] signatureBytes = Sign(Encoding.UTF8.GetBytes(stringToSign), signingKey);
            string signature = BitConverter.ToString(signatureBytes).Replace("-", "").ToLower();

            // Construct authorization header
            string authorizationHeader = $"{algorithm} Credential={accessKey}/{credentialScope}, SignedHeaders={signedHeaders}, Signature={signature}";

            // Return authorization header
            return authorizationHeader;
        }


        private byte[] GetSignatureKey(string key, string dateStamp, string regionName, string serviceName)
        {
            byte[] kDate = Sign(Encoding.UTF8.GetBytes($"AWS4{key}"), Encoding.UTF8.GetBytes(dateStamp));
            byte[] kRegion = Sign(kDate, Encoding.UTF8.GetBytes(regionName));
            byte[] kService = Sign(kRegion, Encoding.UTF8.GetBytes(serviceName));
            byte[] kSigning = Sign(kService, Encoding.UTF8.GetBytes("aws4_request"));
            return kSigning;
        }

        private byte[] Sign(byte[] data, byte[] key)
        {
            using(var hmac = new HMACSHA256(key))
            {
                return hmac.ComputeHash(data);
            }
        }

        private string Hash(string data)
        {
            using(var sha256 = SHA256.Create())
            {
                var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(data));
                return BitConverter.ToString(hashedBytes).Replace("-", "").ToLower();
            }
        }
    }
}