using MediatR;
using PetCareBooking.Application.Common.Models;
using PetCareBooking.Application.DTOs.Staff;

namespace PetCareBooking.Application.Features.Staffs.Queries.GetStaffProfile
{
    public class GetStaffProfileQuery : IRequest<ApiResponse<StaffProfileResponseDto>>
    {
    }
}
