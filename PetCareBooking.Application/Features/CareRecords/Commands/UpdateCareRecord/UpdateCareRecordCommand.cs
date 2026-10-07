using MediatR;
using PetCareBooking.Application.Common.Models;
using System.Text.Json.Serialization;

namespace PetCareBooking.Application.Features.CareRecords.Commands.UpdateCareRecord
{
    public class UpdateCareRecordCommand : IRequest<ApiResponse<Guid>>
    {
        [JsonIgnore]
        public Guid Id { get; set; }

        public DateTime? RecordDate { get; set; }
        public string? HealthStatus { get; set; }
        public string? Note { get; set; }
        public string? ImageUrl { get; set; }
    }
}
