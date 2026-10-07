using MediatR;
using PetCareBooking.Application.Common.Models;
using PetCareBooking.Application.DTOs.CareRecord;

namespace PetCareBooking.Application.Features.CareRecords.Queries.GetCareRecordById
{
    public class GetCareRecordByIdQuery : IRequest<ApiResponse<CareRecordResponseDTO>>
    {
        public Guid Id { get; set; }

        public GetCareRecordByIdQuery()
        {
        }

        public GetCareRecordByIdQuery(Guid id)
        {
            Id = id;
        }
    }
}
