using MediatR;
using PetCareBooking.Application.Common.Models;
using PetCareBooking.Application.DTOs.Staff;
using PetCareBooking.Domain.Enums;

namespace PetCareBooking.Application.Features.Staffs.Queries.GetStaffsWithPagination
{
    public class GetStaffsWithPaginationQuery : IRequest<ApiResponse<PagedResult<StaffListResponseDto>>>
    {
        public string? SearchTerm { get; set; }
        public StaffStatus? Status { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }
}