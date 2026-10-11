using Microsoft.AspNetCore.Http;

namespace PetCareBooking.Application.DTOs.Media
{
    public class UploadMediaRequestDto
    {
        public IFormFile File { get; set; } = null!;
        public string? Folder { get; set; } = "general";
    }
}

