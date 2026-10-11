using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using PetCareBooking.Application.Common.Models;
using PetCareBooking.Application.DTOs.Media;
using PetCareBooking.Application.Interfaces;

namespace PetCareBooking.Infrastructure.Services
{
    public class CloudinaryService : ICloudinaryService
    {
        private readonly Cloudinary _cloudinary;

        public CloudinaryService(IConfiguration configuration)
        {
            var cloudName = configuration["CloudinarySettings:CloudName"];
            var apiKey = configuration["CloudinarySettings:ApiKey"];
            var apiSecret = configuration["CloudinarySettings:ApiSecret"];

            var account = new Account(cloudName, apiKey, apiSecret);
            _cloudinary = new Cloudinary(account);
            _cloudinary.Api.Secure = true;
        }

        public async Task<ApiResponse<UploadMediaResponseDto>> UploadImageAsync(IFormFile file, string? folder = "petcare")
        {
            if (file == null || file.Length == 0)
            {
                return new ApiResponse<UploadMediaResponseDto>
                {
                    IsSuccess = false,
                    StatusCode = StatusCodes.Status400BadRequest,
                    Message = "No file uploaded or file is empty."
                };
            }

            // Validate file extension
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp", ".gif" };
            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!allowedExtensions.Contains(extension))
            {
                return new ApiResponse<UploadMediaResponseDto>
                {
                    IsSuccess = false,
                    StatusCode = StatusCodes.Status400BadRequest,
                    Message = "Invalid image file format. Only JPG, JPEG, PNG, WEBP, and GIF are allowed."
                };
            }

            // Limit file size to 10MB
            if (file.Length > 10 * 1024 * 1024)
            {
                return new ApiResponse<UploadMediaResponseDto>
                {
                    IsSuccess = false,
                    StatusCode = StatusCodes.Status400BadRequest,
                    Message = "File size exceeds the 10MB limit."
                };
            }

            try
            {
                await using var stream = file.OpenReadStream();
                var uploadFolder = string.IsNullOrWhiteSpace(folder) ? "petcare" : $"petcare/{folder.Trim().Trim('/')}";

                var uploadParams = new ImageUploadParams
                {
                    File = new FileDescription(file.FileName, stream),
                    Folder = uploadFolder,
                    Transformation = new Transformation().Quality("auto").FetchFormat("auto")
                };

                var uploadResult = await _cloudinary.UploadAsync(uploadParams);

                if (uploadResult.Error != null)
                {
                    return new ApiResponse<UploadMediaResponseDto>
                    {
                        IsSuccess = false,
                        StatusCode = StatusCodes.Status400BadRequest,
                        Message = $"Cloudinary upload failed: {uploadResult.Error.Message}"
                    };
                }

                return new ApiResponse<UploadMediaResponseDto>
                {
                    IsSuccess = true,
                    StatusCode = StatusCodes.Status200OK,
                    Message = "Image uploaded successfully.",
                    Result = new UploadMediaResponseDto
                    {
                        Url = uploadResult.SecureUrl?.ToString() ?? uploadResult.Url?.ToString() ?? string.Empty,
                        PublicId = uploadResult.PublicId
                    }
                };
            }
            catch (Exception ex)
            {
                return new ApiResponse<UploadMediaResponseDto>
                {
                    IsSuccess = false,
                    StatusCode = StatusCodes.Status500InternalServerError,
                    Message = $"An error occurred while uploading image: {ex.Message}"
                };
            }
        }

        public async Task<ApiResponse<bool>> DeleteImageAsync(string publicId)
        {
            if (string.IsNullOrWhiteSpace(publicId))
            {
                return new ApiResponse<bool>
                {
                    IsSuccess = false,
                    StatusCode = StatusCodes.Status400BadRequest,
                    Message = "PublicId cannot be empty."
                };
            }

            try
            {
                var deleteParams = new DeletionParams(publicId);
                var result = await _cloudinary.DestroyAsync(deleteParams);

                if (result.Result == "ok")
                {
                    return new ApiResponse<bool>
                    {
                        IsSuccess = true,
                        StatusCode = StatusCodes.Status200OK,
                        Message = "Image deleted successfully.",
                        Result = true
                    };
                }

                return new ApiResponse<bool>
                {
                    IsSuccess = false,
                    StatusCode = StatusCodes.Status400BadRequest,
                    Message = $"Failed to delete image: {result.Result}",
                    Result = false
                };
            }
            catch (Exception ex)
            {
                return new ApiResponse<bool>
                {
                    IsSuccess = false,
                    StatusCode = StatusCodes.Status500InternalServerError,
                    Message = $"An error occurred while deleting image: {ex.Message}",
                    Result = false
                };
            }
        }
    }
}

