using MediatR;
using PetCareBooking.Application.Common.Models;
using PetCareBooking.Application.DTOs.Customer;

namespace PetCareBooking.Application.Features.Customers.Queries.GetProfile
{
    public class GetCustomerProfileQuery : IRequest<ApiResponse<CustomerProfileDto>>
    {
    }
}
