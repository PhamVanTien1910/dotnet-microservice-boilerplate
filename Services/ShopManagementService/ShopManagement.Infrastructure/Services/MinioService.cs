using BuildingBlocks.Domain.Exceptions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Minio;
using Minio.DataModel.Args;
using ShopManagement.Application.Interfaces;
using ShopManagement.Infrastructure.Configurations;

namespace ShopManagement.Infrastructure.Services
{
    public class MinioService : IUploadService
    {
        private readonly IMinioClient _minioClient;
        private readonly MinioSettings _minioSettings;

        public MinioService(IMinioClient minioClient, IOptions<MinioSettings> minioSettings)
        {
            _minioClient = minioClient;
            _minioSettings = minioSettings.Value;
        }

        public async Task<string> UploadImageAsync(IFormFile imageFile, string fileName, string contentType, long contentLength, string bucketName)
        {
            try
            {
                await EnsureBucketExistsAsync(bucketName);

                using var stream = imageFile.OpenReadStream();
                var putObjectArgs = new PutObjectArgs()
                    .WithBucket(bucketName)
                    .WithObject(fileName)
                    .WithStreamData(stream)
                    .WithObjectSize(contentLength)
                    .WithContentType(contentType);

                await _minioClient.PutObjectAsync(putObjectArgs).ConfigureAwait(false);

                return $"{_minioSettings.PublicUrl}/{bucketName}/{fileName}";
            }
            catch (Minio.Exceptions.MinioException e)
            {
                throw new Exception($"MinIO Error: {e.Message}", e);
            }
            catch (Exception e)
            {
                throw new Exception($"General Error: {e.Message}", e);
            }
        }

        public async Task<string> UploadFileAsync(IFormFile file, string fileName, string contentType, long contentLength, string bucketName)
        {
            try
            {
                await EnsureBucketExistsAsync(bucketName);

                using var stream = file.OpenReadStream();
                var putObjectArgs = new PutObjectArgs()
                    .WithBucket(bucketName)
                    .WithObject(fileName)
                    .WithStreamData(stream)
                    .WithObjectSize(contentLength)
                    .WithContentType(contentType);

                await _minioClient.PutObjectAsync(putObjectArgs).ConfigureAwait(false);

                return $"{_minioSettings.PublicUrl}/{bucketName}/{fileName}";
            }
            catch (Minio.Exceptions.MinioException e)
            {
                throw new Exception($"MinIO Error: {e.Message}", e);
            }
            catch (Exception e)
            {
                throw new Exception($"General Error: {e.Message}", e);
            }
        }

        public async Task DeleteFileAsync(string fileUrl, string bucketName)
        {
            try
            {
                if(string.IsNullOrEmpty(fileUrl))
                    return;
                string objectName;
                var expectedPrefix = $"{_minioSettings.PublicUrl}/{bucketName}/";

                if (!fileUrl.StartsWith(expectedPrefix))
                    throw new BadRequestException("The file URL does not match the expected format.");

                objectName = fileUrl.Substring(expectedPrefix.Length);
                var removeObjectArgs = new RemoveObjectArgs()
                    .WithBucket(bucketName)
                    .WithObject(objectName);

                await _minioClient.RemoveObjectAsync(removeObjectArgs).ConfigureAwait(false);
            }
            catch (Minio.Exceptions.MinioException e)
            {
                throw new Exception($"MinIO Error: {e.Message}", e);
            }
            catch (Exception e)
            {
                throw new Exception($"General Error: {e.Message}", e);
            }
        }

        public async Task DeleteImageAsync(string imageUrl, string bucketName)
        {
            try
            {
                if(string.IsNullOrEmpty(imageUrl))
                    return;
                string objectName;
                var expectedPrefix = $"{_minioSettings.PublicUrl}/{bucketName}/";
                if (!imageUrl.StartsWith(expectedPrefix))
                    throw new BadRequestException("The image URL does not match the expected format.");
                objectName = imageUrl.Substring(expectedPrefix.Length);
                var removeObjectArgs = new RemoveObjectArgs()
                    .WithBucket(bucketName)
                    .WithObject(objectName);

                await _minioClient.RemoveObjectAsync(removeObjectArgs).ConfigureAwait(false);
            }
            catch (Minio.Exceptions.MinioException e)
            {
                throw new Exception($"MinIO Error: {e.Message}", e);
            }
            catch (Exception e)
            {
                throw new Exception($"General Error: {e.Message}", e);
            }
        }

        private async Task EnsureBucketExistsAsync(string bucketName)
        {
            var bucketExistsArgs = new BucketExistsArgs().WithBucket(bucketName);
            bool found = await _minioClient.BucketExistsAsync(bucketExistsArgs);

            if (!found)
            {
                var makeBucketArgs = new MakeBucketArgs().WithBucket(bucketName);
                await _minioClient.MakeBucketAsync(makeBucketArgs);

                // Set bucket policy to public (read-only)
                string policyJson = $@"
                {{
                    ""Version"": ""2012-10-17"",
                    ""Statement"": [
                        {{
                            ""Effect"": ""Allow"",
                            ""Principal"": {{""AWS"": [""*""]}},
                            ""Action"": [""s3:GetObject""],
                            ""Resource"": [""arn:aws:s3:::{bucketName}/*""]
                        }}
                    ]
                }}";

                var setPolicyArgs = new SetPolicyArgs()
                    .WithBucket(bucketName)
                    .WithPolicy(policyJson);

                await _minioClient.SetPolicyAsync(setPolicyArgs);
            }
        }
    }
}