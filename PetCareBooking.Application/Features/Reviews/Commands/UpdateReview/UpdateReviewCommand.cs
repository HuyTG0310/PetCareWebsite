using MediatR;
using PetCareBooking.Application.Common.Models;
using System.Text.Json.Serialization;

namespace PetCareBooking.Application.Features.Reviews.Commands.UpdateReview
{
    public class UpdateReviewCommand : IRequest<ApiResponse<bool>>
    {
        [JsonIgnore]
        public Guid Id { get; set; }

        public int Rating { get; set; }
        public string? Comment { get; set; }
    }
}