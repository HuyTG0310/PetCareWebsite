using Microsoft.AspNetCore.Http;
using PetCareBooking.Application.Common.Models;
using PetCareBooking.Application.DTOs.Media;

namespace PetCareBooking.Application.Interfaces
{
    public interface ICloudinaryService
    {
        Task<ApiResponse<UploadMediaResponseDto>> UploadImageAsync(IFormFile file, string? folder = "petcare");
        Task<ApiResponse<bool>> DeleteImageAsync(string publicId);
    }
}

