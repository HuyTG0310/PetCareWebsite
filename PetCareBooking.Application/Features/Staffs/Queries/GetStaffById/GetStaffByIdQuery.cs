using MediatR;
using PetCareBooking.Application.Common.Models;
using PetCareBooking.Application.DTOs.Staff;

namespace PetCareBooking.Application.Features.Staffs.Queries.GetStaffById
{
    public class GetStaffByIdQuery : IRequest<ApiResponse<StaffDetailResponseDto>>
    {
        public Guid Id { get; set; }

        public GetStaffByIdQuery(Guid id)
        {
            Id = id;
        }
    }
}
