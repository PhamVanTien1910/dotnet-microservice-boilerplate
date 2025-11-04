using Microsoft.AspNetCore.Http;

namespace ShopManagement.Application.Interfaces
{
    public interface IUploadService
    {
        Task<string> UploadImageAsync(IFormFile imageFile, string fileName, string contentType, long contentLength, string bucketName);
        Task DeleteImageAsync(string imageUrl, string bucketName);
        Task<string> UploadFileAsync(IFormFile file, string fileName, string contentType, long contentLength, string bucketName);
        Task DeleteFileAsync(string fileUrl, string bucketName);
    }
}