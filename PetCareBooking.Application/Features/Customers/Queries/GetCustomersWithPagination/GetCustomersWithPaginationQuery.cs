using MediatR;
using PetCareBooking.Application.Common.Models;
using PetCareBooking.Application.DTOs.Customer;

namespace PetCareBooking.Application.Features.Customers.Queries.GetCustomersWithPagination
{
    public class GetCustomersWithPaginationQuery : IRequest<ApiResponse<PagedResult<CustomerListResponseDTO>>>
    {
        public string? SearchTerm { get; set; }
        public bool? IsActive { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }
}
