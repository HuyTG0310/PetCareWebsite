using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PetCareBooking.Application.Common.Models;
using PetCareBooking.Application.DTOs.Media;
using PetCareBooking.Application.Interfaces;

namespace PetCareBooking.API.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class MediaController : ControllerBase
    {
        private readonly ICloudinaryService _cloudinaryService;

        public MediaController(ICloudinaryService cloudinaryService)
        {
            _cloudinaryService = cloudinaryService;
        }


        [HttpPost("upload")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> UploadImage([FromForm] UploadMediaRequestDto request)
        {
            if (request == null || request.File == null || request.File.Length == 0)
            {
                return BadRequest(new ApiResponse<UploadMediaResponseDto>
                {
                    IsSuccess = false,
                    StatusCode = StatusCodes.Status400BadRequest,
                    Message = "Please select an image file to upload."
                });
            }

            var response = await _cloudinaryService.UploadImageAsync(request.File, request.Folder);
            return StatusCode(response.StatusCode, response);
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteImage([FromQuery] string publicId)
        {
            if (string.IsNullOrWhiteSpace(publicId))
            {
                return BadRequest(new ApiResponse<bool>
                {
                    IsSuccess = false,
                    StatusCode = StatusCodes.Status400BadRequest,
                    Message = "PublicId is required."
                });
            }

            var response = await _cloudinaryService.DeleteImageAsync(publicId);
            return StatusCode(response.StatusCode, response);
        }
    }
}

